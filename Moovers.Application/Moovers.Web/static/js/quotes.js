/// <reference path="references.js" />
/*!
    quotes.js
*/

/*global Utility,QuoteComment,Keys,confirm,Accounts*/
var Quotes = {
    quoteid: null,
    lookup: null,
    saveInterval: 1000,
    saveFunctions: [],
    competitors: [],

    showSummary: function() {
        return _.isUndefined($.cookie("show-quote-summary")) ? true : Utility.parseBool($.cookie("show-quote-summary"));
    },
    bindSave: function(func) {
        var success = function() { };
        var error = function() { };
        var always = function() { };
        Quotes.saveFunctions.push(function() {
            func(success, error, always);
        });
    },
    save: function() {
        _.each(Quotes.saveFunctions, function(func) {
            func();
        });
    },
    templates: {
        quicklook: "#quote-quicklook-template",
        comment: "#quote-comment-template"
    },
    elements: {
        quicklook: "#quote-quicklook",
        comments: ".user-comment-container",
        addcomment: "#add-comment",
        commentText: "#comment-text",
        editComment: "#comment-edit-container",
        saveComment: "#comment-container",
        loadingComment: "#add-comment-loader"
    },
    urls: {
        quicklook: SERVER.baseUrl + "Quote/GetQuicklook",
        postComment: SERVER.baseUrl + "Quote/AddComment",
        getComments: SERVER.baseUrl + "Quote/GetComments",
        editComment: SERVER.baseUrl + "Quote/EditComment"
    },
    templateComment: function(comment) {
        var element = $($.parseHTML(Quotes.templates.comment(comment)));
        $("#user-no-comments").remove();
        Quotes.elements.comments.append(element);
        Quotes.elements.comments[0].scrollTop = Quotes.elements.comments[0].scrollHeight;
    },
    updateQuicklook: function(json) {
        if (!json) {
            $.post(Quotes.urls.quicklook, { quoteid: Quotes.quoteid }, function(resp) {
                Quotes.updateQuicklook(resp);
            });
        }
        else {
            Quotes.elements.quicklook.empty().append(Quotes.templates.quicklook(json));
        }
    },
    addCompetitor: function(competitorid, name) {
        var url = SERVER.baseUrl + "Quote/AddCompetitor";
        var data = {
            quoteid: Quotes.quoteid,
            competitorID: competitorid,
            name: name
        };

        $.post(url, data, function(resp) {
            $("#competitor-container").find(".selection-badge[data-id=" + competitorid + "]").data("id", resp).find(".icon-remove").show();
        });
    },
    removeCompetitor: function(relid) {
        var url = SERVER.baseUrl + "Quote/RemoveCompetitor";
        var data = {
            quoteid: Quotes.quoteid,
            relid: relid
        };

        $.post(url, data);
    },
    showLoadingComment: function() {
        Quotes.elements.editComment.addClass("disabled").attr("disabled", "disabled");
        Quotes.elements.addcomment.addClass("disabled").attr("disabled", "disabled");
        Quotes.elements.loadingComment.show();
    },
    hideLoadingComment: function() {
        Quotes.elements.editComment.removeClass("disabled").removeAttr("disabled", "disabled");
        Quotes.elements.addcomment.removeClass("disabled").removeAttr("disabled", "disabled");
        Quotes.elements.loadingComment.hide();
    },
    init: function(opp, competitors) {
        Utility.initBase(Quotes);
        Quotes.quoteid = opp.QuoteID;
        Quotes.lookup = opp.Lookup;
        Quotes.competitors = competitors;

        if ($.cookie("last-quote") !== Quotes.quoteid) {
            $.cookie("last-quote", Quotes.quoteid, { path: "/" });
            $.cookie("show-quote-summary", false, { path: "/" });
        }

        Quotes.elements.loadingComment.hide();
        Quotes.initCompetitors();

        var summaryPanel = $("#shipping-info");
        var summaryBtn = $("#toggle-shipping-info");

        summaryBtn.click(function() {
            var visible = summaryPanel.is(":visible");
            $.cookie("show-quote-summary", !visible, { path: "/" });
            summaryPanel.slideToggle("slow");
            summaryBtn.find("i").toggleClass("icon-chevron-up icon-chevron-down");
        });

        Accounts.initAddModals(function() {
            window.location.reload();
        }, false);

        // set initial templates that don't exist on new quotes
        Quotes.updateQuicklook();
        $.post(Quotes.urls.getComments, { quoteid: Quotes.quoteid }, function(resp) {
            var items = _.map(resp, function(c) {
                return new QuoteComment(c);
            });
            _.each(items, function(comment) {
                Quotes.templateComment(comment);
            });

            Quotes.elements.comments[0].scrollTop = Quotes.elements.comments[0].scrollHeight;
        });

        var changeOwner = $("#change-owner-modal").modal({ show: false });
        $("#change-user-button").click(function() {
            changeOwner.modal("show");
            return false;
        });

        $("#save-button").click(function() {
            Quotes.save();
            var href = $(this).attr("href");
            Utility.showOverlay();

            var isRedirecting = false;
            setInterval(function() {
                if (!$.ajaxq.isRunning() && !isRedirecting) {
                    isRedirecting = true;
                    window.location = href;
                }
            }, 100);
        });

        var cancelEditComment = function() {
            Quotes.elements.editComment.hide();
            Quotes.elements.saveComment.show();
            Quotes.elements.commentText.val("");
        };

        Quotes.elements.comments.on("click", ".edit-icon", function() {
            var commentid = $(this).data("commentid");
            var text = $(this).data("text");
            Quotes.elements.saveComment.hide();
            Quotes.elements.editComment.show();
            Quotes.elements.editComment.find("textarea").val(text).focus().select();
            Quotes.elements.editComment.find("#save-comment").data("id", commentid);
            return false;
        });

        Quotes.elements.comments.on("click", ".delete-icon", function() {
            var commentid = $(this).data("commentid");
            cancelEditComment();
            if (confirm("Are you sure you want to remove this comment? This can't be undone.")) {
                Quotes.showLoadingComment();
                var ajax = $.post(Quotes.urls.editComment, { commentid: commentid, text: "", "delete": true });
                ajax.always(function() {
                });
                Quotes.hideLoadingComment();

                $(this).closest(".user-comment").slideUp();
            }

            return false;
        });

        // mark quote as deferred, lost, etc.
        var changeStatusModal = $("#status-modal").modal({ show: false });
        $("#change-quote-status").find("a.action").click(function() {
            $(this).siblings("input[type=submit]").click();
            return false;
        });

        $("#change-quote-status").submit(false);

        $("#change-quote-status").find("[type=submit]").click(function() {
            var action = $(this).val();
            var title = $(this).data("title");
            changeStatusModal.find("[name=action]").val(action);
            changeStatusModal.find(".title").text(title);
            changeStatusModal.find("[type=submit]").val(title);
            changeStatusModal.modal("show");
            changeStatusModal.find("textarea").first().focus();
            return false;
        });

        var movedateModal = $("#change-movedate-modal").modal({ show: false });
        $("#edit-move-date").click(function() {
            movedateModal.modal("show");
            return false;
        });

        // on comments, CTRL + Enter submits the comment.
        Quotes.elements.commentText.keydown(function(e) {
            if (e.keyCode === Keys.ENTER && e.shiftKey) {
                return true;
            }
            if (e.keyCode === Keys.ENTER) {
                Quotes.elements.addcomment.click();
                return false;
            }
        });

        Quotes.elements.editComment.find("#save-comment").click(function() {
            var text = Quotes.elements.editComment.find("textarea").val();
            var id = $(this).data("id");

            Quotes.showLoadingComment();
            var ajax = $.post(Quotes.urls.editComment, { commentid: id, text: text }, function(resp) {
                var comment = new QuoteComment(resp);
                var el = Quotes.elements.comments.find("[data-commentid=" + comment.CommentID + "]");
                el.replaceWith(Quotes.templates.comment(comment));
                cancelEditComment();
            });
            ajax.always(function() {
                Quotes.hideLoadingComment();
            });

            return false;
        });

        Quotes.elements.editComment.find(".cancel").click(function() {
            cancelEditComment();
            return false;
        });

        var isAdding = false;
        Quotes.elements.addcomment.click(function() {
            var text = Utility.htmlEncode(Quotes.elements.commentText.val());
            var id = Quotes.quoteid;

            if (!text || isAdding) {
                return;
            }

            isAdding = true;
            Quotes.showLoadingComment();
            $.post(Quotes.urls.postComment, { quoteid: id, text: text }, function(resp) {
                Quotes.templateComment(new QuoteComment(resp));
                Quotes.hideLoadingComment();
                Quotes.elements.commentText.val("");
                isAdding = false;
            });
        });

        setInterval(function() {
            Quotes.save();
        }, Quotes.saveInterval);
    },
    initCompetitors: function() {
        var tmpl = _.template($("#competitor-template").html());
        $("#competitor-container").append(_.map(Quotes.competitors, function(c) {
            return tmpl(c);
        }).join(""));


        $("#competitor-select").click(function() {
            $(this).find("select").click();
        });
        $("#competitor-select select").click(function(e) {
            e.stopPropagation();
        });

        $("#competitor-select select").change(function() {
            var id = $(this).val();
            var el = $(this).find("[value=" + id + "]");
            var name = el.text();

            if (name === "Other") {
                name = window.prompt("What is the name of the competitor?");
            }

            if (!name) {
                $(this).val("SELECT");
                return;
            }

            el.remove();
            var markup = $($.parseHTML(tmpl({
                Name: name,
                ID: id
            })));

            markup.find(".icon-remove").hide();
            $("#competitor-container").append(markup);

            Quotes.addCompetitor(id, name);
            $(this).val("SELECT");
        });

        $("#competitor-container").on("click", ".icon-remove", function() {
            var id = $(this).closest(".selection-badge").data("id");
            Quotes.removeCompetitor(id);
            $(this).closest(".selection-badge").remove();
        });
    }
};
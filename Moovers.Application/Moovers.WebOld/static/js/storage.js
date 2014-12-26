/// <reference path="references.js" />
/*global SearchFunctions,Keys, Storage, PaymentModal,Utility*/
/*!
    storage.js
*/
 var Storage = {
    InventoryItems: [],
    elements: {
        comments: ".user-comment-container",
        addcomment: "#add-comment",
        commentText: "#comment-text",
        editComment: "#comment-edit-container",
        saveComment: "#comment-container",
        loadingComment: "#add-comment-loader",
        editCommentText: "#comment-edit-text"
    },
    getItem: function(itemName) {
        function findItem(i) {
            return i.ItemID === itemName ||
                $.trim((i.Name || "").toLowerCase()) === $.trim((itemName || "").toLowerCase());
        }

        var item = _.find(Storage.InventoryItems, findItem);
        var custom = _.find(Storage.InventoryItems, findItem);
        return item || custom;
    },

    templateComment: function(comment) {
        var element = $($.parseHTML(Storage.commentTemplate(comment)));
        $("#user-no-comments").remove();
        var container = $(".user-comment-container").append(element);
        container[0].scrollTop = container[0].scrollHeight;
    },
    showLoadingComment: function () {
        Storage.elements.editComment.addClass("disabled").attr("disabled", "disabled");
        Storage.elements.addcomment.addClass("disabled").attr("disabled", "disabled");
        Storage.elements.loadingComment.show();
    },
    hideLoadingComment: function () {
        Storage.elements.editComment.removeClass("disabled").removeAttr("disabled", "disabled");
        Storage.elements.addcomment.removeClass("disabled").removeAttr("disabled", "disabled");
        Storage.elements.loadingComment.hide();
    },

    initView: function(storageid) {
        Utility.initBase(Storage);
        this.commentTemplate = _.template($("#storage-comment-template").html());
        $("#email-log").find("tr.toggle").click(function() {
            $(this).next(".hidden").toggle();

        });

        $("#edit-notes-link").click(function() {
            $("#edit-notes").show();
            $("#notes").hide();
        });

        $("#cancel-edit-notes-link").click(function() {
            $("#edit-notes").hide();
            $("#notes").show();
        });

        PaymentModal.init($(".add-payment-modal"));
        $("#add-payment").click(function() {
            PaymentModal.show();
            return false;
        });

        var changeAmountModal = $("#change-amount-modal").modal({ show: false });
        $("#change-monthly-payment").click(function() {
            changeAmountModal.modal("show");
            return false;
        });

        $("#invoice-table form").submit(function() {
            return confirm("Are you sure you want to cancel this invoice? This cannot be undone.");
        });

        var addCardModal = $("#add-card-modal").modal({ show: false });
        var newcard = addCardModal.find("#new-card").hide();


        var isRunning = false;
        addCardModal.find("form").on("submit", function(e) {
            e.preventDefault();

            if (isRunning) {
                return;
            }

            var errorBox = addCardModal.find(".errors");
            var loader = addCardModal.find("#cc-submit-loader");
            var form = $(this);
            var url = form.attr("action");
            var data = form.serialize();
            errorBox.empty().hide();

            loader.show();
            var submitButton = form.find("[type=submit]").prop("disabled");
            isRunning = true;

            var post = $.post(url, data);
            post.success(function(e) {
                if (e.Errors && e.Errors.length > 0) {
                    errorBox.append(e.Errors[0].ErrorMessage);
                    errorBox.show();
                }
                else {
                    window.location.reload();
                }
            });

            post.fail(function() {
                errorBox.append("Error processing card");
                errorBox.show();
            });
            
            post.always(function() {
                loader.hide();
                submitButton = form.find("[type=submit]").removeProp("disabled");
                isRunning = false;
            });
        });

        $("input[name=paymentType]").change(function() {
            var isnew = $(this).val() === "NEW_CARD";
            newcard.toggle(isnew);
            newcard.find("[name=name]").toggleAttr("required", isnew);
            newcard.find("[name=cardnumber]").toggleAttr("required", isnew);
        });

        $("#add-card").click(function() {
            addCardModal.modal("show");
            return false;
        });

        $("#btn-close").click(function() {
            return window.confirm("Are you sure you want to close this account?");
        });

        var opts = {
            readAsMap: { ".*": "DataURL" },
            dragClass: "filedrop",
            on: {
                loadstart: function() {
                    Utility.showOverlay();
                },
                load: function(e, file) {
                    var src = e.target.result;
                    var data = src.split(",")[1];
                    $.post(SERVER.baseUrl + "Storage/UploadFile", {
                        data: data,
                        name: file.name,
                        contentType: file.type,
                        storageid: $("[name=current-storage-id]").val()
                    }, function() {
                        window.location.reload();
                    });
                }
            }
        };

        // hack to make both section elements the same height
        var newHeight = _.max($("#job-specifics .section").map(function() { return $(this).height(); }).get());
        $("#job-specifics .section").height(newHeight);

        $("#add-storage-file, body").fileReaderJS(opts);

        

        $.post(SERVER.baseUrl + "Storage/GetComments", { storageid: storageid }, function(resp) {
            var items = _.map(resp, function(c) {
                return new StorageComment(c);
            });
            _.each(items, function(comment) {
                Storage.templateComment(comment);
            });

            var container = $(".user-comment-container");
            container[0].scrollTop = container[0].scrollHeight;
        });

        var loader = $("#add-comment-loader").hide();

        $("#save-comment").click(function() {
            var text = $(".save-comment-container textarea").val();
            var id = $(this).data("id");
            loader.show();
            var ajax = $.post(SERVER.baseUrl + "Storage/EditComment", { commentid: id, text: text }, function(resp) {
                var comment = new StorageComment(resp);
                var el = $(".user-comment-container").find("[data-commentid=" + comment.CommentID + "]");
                el.replaceWith(Storage.commentTemplate(comment));
                cancelEditComment();
            });
            ajax.always(function() {
                loader.hide();
            });

            return false;
        });

        // on comments, CTRL + Enter submits the comment.
        $("#comment-text").keydown(function(e) {
            if (e.keyCode === Keys.ENTER && e.shiftKey) {
                return true;
            }
            if (e.keyCode === Keys.ENTER) {
                $("#add-comment").click();
                return false;
            }
        });

        var isAdding = false;
        $("#add-comment").click(function() {
            var text = Utility.htmlEncode($("#comment-text").val());
            var id = storageid;

            if (!text || isAdding) { return; }

            isAdding = true;
            loader.show();
            $.post(SERVER.baseUrl + "Storage/AddComment", { storageid: id, text: text }, function(resp) {
                Storage.templateComment(new StorageComment(resp));
                $("#comment-text").val("");
                isAdding = false;
                loader.hide();
            });
        });

        var cancelEditComment = function () {
            Storage.elements.editComment.hide();
            Storage.elements.saveComment.show();
            Storage.elements.commentText.val("");
            Storage.elements.editCommentText.val("");
        };

        Storage.elements.comments.on("click", ".edit-icon", function () {
            var commentid = $(this).data("commentid");
            var text = $(this).data("text");
            Storage.elements.saveComment.hide();
            Storage.elements.editComment.show();
            Storage.elements.editComment.find("textarea").val(text).focus().select();
            Storage.elements.editComment.find("#save-comment").data("id", commentid);
            return false;
        });

        Storage.elements.comments.on("click", ".delete-icon", function () {
            var commentid = $(this).data("commentid");
            cancelEditComment();
            if (confirm("Are you sure you want to remove this comment? This can't be undone.")) {
                Storage.showLoadingComment();
                var ajax = $.post(SERVER.baseUrl + "Storage/EditComment", { commentid: commentid, text: "", "delete": true });
                ajax.always(function () {
                });
                Storage.hideLoadingComment();

                $(this).closest(".user-comment").slideUp();
            }

            return false;
        });

        Storage.elements.editComment.find(".cancel").click(function () {
            cancelEditComment();
            return false; 
        });


    },

    init: function() {
        var data = _.map(Storage.InventoryItems, function(c) {
            return {
                value: c.Name,
                alts: _.flatten([[(c.KeyCode || "").toString(), c.PluralName], c.Aliases])
            };
        });

        $("#inventory-add").autocomplete({
            lookup: data,
            partialMatch: true,
            selectFirst: true,
            maxSuggestions: 10,
            orderBy: SearchFunctions.itemAutocompleteSort,
            onSelect: function(val, originalEvent) {
                var item = Storage.getItem(val);
                $("#add-item-form [name=itemid]").val(item.ItemID);
                if (originalEvent.keyCode === Keys.TAB) {
                    originalEvent.preventDefault();
                }

                $("#add-item-form [name=count]").focus();

                return false;
            }
        });
    }
};

window.StorageComment = (function() {

    function StorageComment(resp) {
        var _ref;
        resp = resp != null ? resp : {};
        this.Text = resp.Text, this.UserName = resp.UserName, this.IsEditable = resp.IsEditable;
        this.CommentID = (_ref = resp.CommentID) != null ? _ref : Utility.randomid();
        if (resp.Date && !resp.Date.getDate) {
            this.Date = new Date(resp.Date);
        } else {
            this.Date = resp.Date;
        }
    }

    StorageComment.prototype.GetDateString = function() {
        if (new Date().toDateString() === this.Date.toDateString()) {
            return prettyDate(this.Date);
        } else {
            return this.Date.format("mmm d, yyyy' at 'h:MM TT");
        }
    };

    StorageComment.prototype.getCommentClass = function() {
        if (SERVER.username === this.UserName) {
            return "logged-in-user";
        } else {
            return "";
        }
    };

    return StorageComment;

})();
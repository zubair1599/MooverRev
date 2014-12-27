(function() {






    /**
     * Elements = {
     *    commentText -- comment textbox
     *    editComment -- edit comment container
     *    addComment -- add comment button
     *    comments -- comment container
     *
     *Urls = {
     *    editComment,
     */
    //

    window.CommentManager = function(elements, urls, constructor) {
        elements.editComment.find(".cancel").click(function() {
            cancelEditComment();
            return false;
        });

        // on comments, CTRL + Enter submits the comment.
        elements.commentText.keydown(function(e) {
            if (e.keyCode === Keys.ENTER && e.shiftKey) {
                return true;
            }
            if (e.keyCode === Keys.ENTER) {
                elements.addcomment.click();
                return false;
            }
        });

        elements.editComment.find("#save-comment").click(function() {
            var text = elements.editComment.find("textarea").val();
            var id = $(this).data("id");

            Quotes.showLoadingComment();
            var ajax = $.post(urls.editComment, { commentid: id, text: text }, function(resp) {
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


        var isAdding = false;
        elements.addcomment.click(function() {
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


        elements.comments.on("click", ".delete-icon", function() {
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

        elements.comments.on("click", ".edit-icon", function() {
            var commentid = $(this).data("commentid");
            var text = $(this).data("text");
            elements.saveComment.hide();
            elements.editComment.show();
            elements.editComment.find("textarea").text(text).focus().select();
            elements.editComment.find("#save-comment").data("id", commentid);
            return false;
        });



    }


});
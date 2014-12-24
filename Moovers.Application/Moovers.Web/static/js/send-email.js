/// <reference path="references.js" />
/*global Utility,SendEmail*/
/*!
    send-email.js
*/
window.SendEmail = {
    templates: {
        addedFile: "#attached-file-template"
    },
    init: function() {
        Utility.initBase(SendEmail);
        $("#template").change(function() {
            var item = JSON.parse($(this).val());
            $("[name=subject]").val(item.Subject);
            $("[name=message]").val(item.Text);
        });

        $("#add-file-list").on("click", "a[data-fileid]", function() {
            var tmpl = SendEmail.templates.addedFile;
            var data = {
                name: $(this).data("name"),
                fileid: $(this).data("fileid")
            };

            var content = $($.parseHTML(tmpl(data))).css("display", "none");
            $("#added-file-list").append(content);
            content.slideDown();
            $(this).closest("li").slideUp();
            return false;
        });

        $("#added-file-list").on("click", ".icon-remove", function() {
            var li = $(this).closest("li").slideUp(function() {
                $(this).remove();
            });
            var id = li.data("fileid");
            $("#add-file-list").find("a[data-fileid='" + id + "']").closest("li").slideDown();
            return false;
        });

        $(".change-email").click(function() {
            var email = $(this).data("email");
            $("[name=to]").val(email);
            return false;
        });

        $("[type=submit]").click(function() {
            Utility.showOverlay();
        });
    }
};
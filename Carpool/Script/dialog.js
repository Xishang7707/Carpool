function dialog({ title, content, ok_call, cancel_call }) {
    var dialog_model = $('<div class="modal fade" id="Dialog_Model" tabindex="-1" role="dialog" aria-labelledby="exampleModalCenterTitle" aria-hidden="true">' +
        '<div class="modal-dialog modal-dialog-centered" role="document">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<h5 class="modal-title" id="exampleModalLongTitle">' + title + '</h5 > ' +
        '<button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span > ' +
        '</button>' +
        '</div>' +
        '<div class="modal-body">' +
        content +
        '</div>' +
        '<div class="modal-footer">' +
        '<button type="button" class="btn btn-secondary btn-cancel" data-dismiss="modal">取消</button>' +
        '<button type="button" class="btn btn-primary btn-ok" data-dismiss="modal">确定</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>');
    $(document.body).append(dialog_model);

    new Vue({
        el: "#Dialog_Model",
        methods: {
            init: function () {
                //$(this.$el).modal({
                //    backdrop: 'static'
                //})
                $(this.$el).on('hidden.bs.modal', function () {
                    $(this).remove();
                })
            },
            cancel: function (e) {
                if (cancel_call)
                    cancel_call(e);
            },
            ok: function (e) {
                if (ok_call)
                    ok_call(e);
            },
            show: function () {
                this.init();
                $(this.$el).modal('show');
            }
        }
    }).show();
}
function dialog_ok({ title, content, ok_call }) {
    var dialog_model = $('<div class="modal fade" id="Dialog_Model" tabindex="-1" role="dialog" aria-labelledby="exampleModalCenterTitle" aria-hidden="true">' +
        '<div class="modal-dialog modal-dialog-centered" role="document">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<h5 class="modal-title" id="exampleModalLongTitle">' + title + '</h5 > ' +
        '<button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span > ' +
        '</button>' +
        '</div>' +
        '<div class="modal-body">' +
        content +
        '</div>' +
        '<div class="modal-footer">' +
        '<button type="button" class="btn btn-primary btn-ok" data-dismiss="modal">确定</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>');
    $(document.body).append(dialog_model);

    new Vue({
        el: "#Dialog_Model",
        methods: {
            init: function () {
                //$(this.$el).modal({
                //    backdrop: 'static'
                //})
                $(this.$el).on('hidden.bs.modal', function () {
                    $(this).remove();
                })
            },
            ok: function (e) {
                if (ok_call)
                    ok_call(e);
            },
            show: function () {
                this.init();
                $(this.$el).modal('show');
            }
        }
    }).show();
}
(function () {
    'use strict';

    angular
        .module('app')
        .controller('nuevaEmpresaController', nuevaEmpresaController);

    nuevaEmpresaController.$inject = ['empresasMessageSender'];

    function nuevaEmpresaController(empresasMessageSender) {
        var vm = this;
        var sender = empresasMessageSender;

        vm.nuevaEmpresa = nuevaEmpresa;

        activate();

        function activate() {
            //...            
        }

        function nuevaEmpresa() {
            sender.nuevaEmpresa(vm.empresa)
                .then(function (data) {
                    toastr.success("Nueva empresa agregada!");
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                });
        }
    }
})();

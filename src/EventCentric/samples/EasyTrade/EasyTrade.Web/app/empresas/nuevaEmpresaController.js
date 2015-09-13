(function () {
    'use strict';

    angular
        .module('app')
        .controller('nuevaEmpresaController', nuevaEmpresaController);

    nuevaEmpresaController.$inject = ['empresasMessageSender', 'utils'];

    function nuevaEmpresaController(empresasMessageSender, utils) {
        var vm = this;
        var sender = empresasMessageSender;

        // View models
        vm.submitText = 'Registrar empresa';

        // Commands
        vm.nuevaEmpresa = nuevaEmpresa;

        activate();

        function activate() {
            //...            
        }

        function nuevaEmpresa() {
            utils.disableSubmitButton();
            vm.submitText = 'Registrando...';

            sender.nuevaEmpresa(vm.empresa)
                .then(function (data) {
                    toastr.success("Nueva empresa agregada!");
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                    vm.submitText = 'Volver a intentar registrar empresa';
                    utils.enableSubmitButton();
                });
        }
    }
})();

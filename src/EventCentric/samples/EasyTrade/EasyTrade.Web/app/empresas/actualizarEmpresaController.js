(function () {
    'use strict';

    angular
        .module('app')
        .controller('actualizarEmpresaController', actualizarEmpresaController);

    actualizarEmpresaController.$inject = ['empresasMessageSender', 'empresasDao', '$state', 'utils'];

    function actualizarEmpresaController(empresasMessageSender, empresasDao, $state, utils) {
        var vm = this;
        var sender = empresasMessageSender;
        var dao = empresasDao;

        // View models
        vm.submitText = 'Actualizar datos';

        // Commands
        vm.actualizarEmpresa = actualizarEmpresa;

        activate();

        function activate() {
            //...            
        }

        function actualizarEmpresa() {
            utils.disableSubmitButton();
            vm.submitText = 'Actualizando...';

            sender.actualizarEmpresa(vm.empresa)
                .then(function (data) {
                    // await eventual consistency
                    toastr.success('La empresa ha sido actualizada correctamente!');
                    vm.submitText = 'Actualizado! Redirigiendo a la lista de empresas...';
                    dao.awaitResult(data.data)
                        .then(function (data) {
                            utils.animateTransitionTo('section.main', 'fadeInLeft', 'fadeOutRight', function () { 
                                $state.go('main');
                            });
                        },
                        function (message) {
                            toastr.error(message.data.exceptionMessage);
                            toastr.warning('La empresa ha sido actualizada correctamente, pero el sistema se está tardando un poco en actualizarse.');
                        });
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                    vm.submitText = 'Volver a intentar actualizar empresa';
                    utils.enableSubmitButton();
                });
        }
    }
})();

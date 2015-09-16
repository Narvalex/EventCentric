(function () {
    'use strict';

    angular
        .module('app')
        .controller('nuevaEmpresaController', nuevaEmpresaController);

    nuevaEmpresaController.$inject = ['empresasMessageSender', 'empresasDao', '$state', 'utils'];

    function nuevaEmpresaController(empresasMessageSender, empresasDao, $state, utils) {
        var vm = this;
        var sender = empresasMessageSender;
        var dao = empresasDao;

        // View models
        vm.submitText = 'Registrar empresa';

        // Commands
        vm.nuevaEmpresa = nuevaEmpresa;
        vm.cancelar = cancelar;

        activate();

        function activate() {
            //...            
        }

        function nuevaEmpresa() {
            utils.disableSubmitButton();
            vm.submitText = 'Registrando...';

            sender.nuevaEmpresa(vm.empresa)
                .then(function (data) {
                    // await eventual consistency
                    toastr.success('La empresa ha sido registrada correctamente!');
                    vm.submitText = 'Registrado! Redirigiendo a la lista de empresas...';
                    dao.awaitResult(data.data)
                        .then(function (data) {
                            utils.animateTransitionTo('section.main', 'fadeInRight', 'zoomOutUp', function () {
                                $state.go('main');
                            });
                        },
                        function (message) {
                            toastr.error(message.data.exceptionMessage);
                            toastr.warning('La empresa ha sido registrada correctamente, pero el sistema se está tardando un poco en actualizarse.');
                            vm.submitText = 'Registrar otra empresa';
                            utils.enableSubmitButton();
                        });
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                    vm.submitText = 'Volver a intentar registrar empresa';
                    utils.enableSubmitButton();
                });
        }

        function cancelar() {
            utils.animateTransitionTo('section.main', 'fadeInRight', 'zoomOutDown', function () {
                $state.go('main');
            });
        }
    }
})();

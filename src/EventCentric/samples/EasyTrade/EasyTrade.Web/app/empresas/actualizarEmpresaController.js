(function () {
    'use strict';

    angular
        .module('app')
        .controller('actualizarEmpresaController', actualizarEmpresaController);

    actualizarEmpresaController.$inject = ['empresasMessageSender', 'empresasDao', '$state', '$stateParams', 'utils'];

    function actualizarEmpresaController(empresasMessageSender, empresasDao, $state, $stateParams, utils) {
        var vm = this;
        var sender = empresasMessageSender;
        var dao = empresasDao;

        // View models
        vm.submitText = 'Recuperando empresa...';
        vm.empresa = {};

        // Commands
        vm.actualizarEmpresa = actualizarEmpresa;
        vm.cancelar = cancelar;

        activate();

        function activate() {
            obtenerEmpresa($stateParams.idEmpresa);
        }

        function obtenerEmpresa(idEmpresa) {
            dao.obtenerEmpresa(idEmpresa)
                .then(function (data) {
                    vm.empresa = data;
                    vm.submitText = 'Actualizar datos';
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                });
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
                            utils.animateTransitionTo('section.main', 'fadeInLeft', 'zoomOutUp', function () { 
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

        function cancelar() {
            utils.animateTransitionTo('section.main', 'fadeInRight', 'zoomOutDown', function () {
                $state.go('main');
            });
        }
    }
})();

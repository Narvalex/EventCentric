'use strict';

angular.module('app').controller('formularioClienteController',
    ['$scope', '$routeParams', '$location',
        function ($scope, $routeParams, $location) {
            var vm = $scope;

            vm.titulo = '';

            vm.onBackBtnClicked = onBackBtnClicked;
            
            activate();

            function activate() {
                verificarSiEsNuevoCliente();
            }

            function verificarSiEsNuevoCliente() {
                if ($routeParams.clienteId == 'nuevo') {
                    // Nuevo cliente
                    vm.titulo = 'Nuevo cliente'
                }
                else {
                    // Cliente existente
                    vm.titulo = 'Actualizar datos del cliente'
                }
            }

            function onBackBtnClicked() {
                $location.path('/home');
            }

//#region Fakes
            
//#endregion
        }
    ]);
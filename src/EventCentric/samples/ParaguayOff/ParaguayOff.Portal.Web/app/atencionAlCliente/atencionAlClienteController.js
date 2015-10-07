'use strict';

angular.module('app').controller('atencionAlClienteController',
    ['$scope', '$location',
        function ($scope, $location) {
            var vm = $scope;

            vm.onNuevoClienteBtnClick = onNuevoClienteBtnClick;
            vm.onClienteSeleccionado = onClienteSeleccionado;

            activate();

            function activate() {
                vm.clientes = obtenerClientes();
            }

            function onNuevoClienteBtnClick() {
                $location.path('/atencion-al-cliente/cliente/nuevo');
            }

            function onClienteSeleccionado(cliente) {
                toastr.info('Usted selecciono a ' + cliente.nombre);
            }

//#region Fakes
            function obtenerClientes() {
                return [
                    {
                        'clienteId': '492c30e8-7d2e-4d86-a8c3-3d8db1f6824b',
                        'nombre': 'Alexis Narváez',
                        'telefono1': '021 505-218',
                        'email': 'narvalex@hotmail.com',
                        'ruc': '3031779-7',
                        'ultimoContacto': '05/05/2015'
                    },
                    {
                        'clienteId': 'd285919a-7f76-4c94-ad9d-4b10cf9ca99f',
                        'nombre': 'Jordan Narváez',
                        'telefono1': '0971 505-218',
                        'email': 'jordan@hotmail.com',
                        'ruc': '100000-7',
                        'ultimoContacto': '04/05/2015'
                    },
                    {
                        'clienteId': '0f9d720e-3661-43a2-a124-aec1cfb13506',
                        'nombre': 'Danubio Narváez',
                        'telefono1': '0981 505-218',
                        'email': 'danubio@hotmail.com',
                        'ruc': '3031779-7',
                        'ultimoContacto': '03/05/2015'
                    },
                    {
                        'clienteId': '5a657063-187d-4dff-9fcd-65ead5856590',
                        'nombre': 'Delicias Japonesas',
                        'telefono1': '0981 505-218',
                        'email': 'deliciajaponesa@hotmail.com',
                        'ruc': '3031779-7',
                        'ultimoContacto': '03/05/2015'
                    }
                ];
            }
//#endregion
        }
    ]);
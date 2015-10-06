'use strict';

angular.module('app').controller('atencionAlClienteController',
    ['$scope',
        function ($scope) {
            var vm = $scope;

            activate();

            function activate() {
                vm.clientes = obtenerClientes();
            }

//#region Fakes
            function obtenerClientes() {
                return [
                    {
                        'nombre': 'Alexis Narváez',
                        'telefono1': '021 505-218',
                        'email': 'narvalex@hotmail.com',
                        'ultimoContacto': '05/05/2015'
                    },
                    {
                        'nombre': 'Jordan Narváez',
                        'telefono1': '0971 505-218',
                        'email': 'jordan@hotmail.com',
                        'ultimoContacto': '04/05/2015'
                    },
                    {
                        'nombre': 'Danubio Narváez',
                        'telefono1': '0981 505-218',
                        'email': 'danubio@hotmail.com',
                        'ultimoContacto': '03/05/2015'
                    }
                ];
            }
//#endregion
        }
    ]);
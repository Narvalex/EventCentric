'use strict';

angular.module('app').controller('loginController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {
            var vm = $scope;

            vm.logIn = function () {
                $rootScope.$broadcast('loggedIn',
                    {
                        payload: ''
                    });
            };
        }
    ]);
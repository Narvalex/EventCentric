'use strict';

angular.module('app').controller('indexController',
    ['$scope',
        function ($scope) {
            var vm = $scope;

            vm.authState = 'authorized';

            $scope.$on('loggedIn',
                function (event, data) {
                    vm.authState = 'authorized';
            });
        }
    ]);
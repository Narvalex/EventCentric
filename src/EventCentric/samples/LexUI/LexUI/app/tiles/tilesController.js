'use strict';

angular.module('app').controller('tilesController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {
            var vm = $scope;

            activate();

            function activate() {
                $('.live-tile').liveTile();

                // destructor;
                $scope.$on('$destroy', function () {
                    $('.live-tile').liveTile('destroy');
                });
            }
        }
    ]);
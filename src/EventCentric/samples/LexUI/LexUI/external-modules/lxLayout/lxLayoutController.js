'use strict';

angular.module('lxLayout').controller('lxLayoutController',
    ['$scope', '$window', '$timeout', '$rootScope',
        function ($scope, $window, $timeout, $rootScope) {

            //$scope.isMenuVisible = true;
            //$scope.isMenuButtonVisible = true;

            $scope.$on('lxMessage-ItemSelected',
                function (event, data) {
                    $scope.routeString = data.route;
                    checkWidth();
                    broadcastMenuState();
                });

            $($window).on('resize.lxLayout', function () {
                $scope.$apply(function () {
                    checkWidth();
                    broadcastMenuState();
                });
            });

            $scope.$on('$destroy', function () {
                $($window).off('resize.lxLayout'); // remove the handler added earlier
            });

            $timeout(function () {
                checkWidth();
                broadcastMenuState();
            }, 0);

            $scope.menuButtonClicked = function () {
                $scope.isMenuVisible = !$scope.isMenuVisible;
                broadcastMenuState();
                //$scope.$apply();
            }

            function checkWidth() {
                var width = Math.max($($window).width(), $window.innerWidth);
                $scope.isMenuVisible = (width >= 768);
                $scope.isMenuButtonVisible = !$scope.isMenuVisible;
            }

            function broadcastMenuState() {
                $rootScope.$broadcast('lxMessage-showMenuStateChanged',
                    {
                        show: $scope.isMenuVisible
                    });
            }
        }
    ]);
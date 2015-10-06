'use strict';

angular.module('lxLayout').controller('lxLayoutController',
    ['$scope', '$window', '$timeout', '$rootScope', '$location',
        function ($scope, $window, $timeout, $rootScope, $location) {

            $scope.$on('lxMessage-itemSelected',
                function (event, data) {
                    $location.path(data.route);
                    checkWidth();
                });
           

            $($window).on('resize.lxLayout', function () {
                $scope.$apply(function () {
                    checkIfSmall();
                });
            });
                
            $scope.$on('$destroy', function () {
                $($window).off('resize.lxLayout'); // remove the handler added earlier
            });

            $timeout(function () {
                checkWidth();
            }, 0);

            $scope.menuButtonClicked = function () {
                $scope.isMenuVisible = !$scope.isMenuVisible;
                broadcastMenuState();
            }

            $scope.goToHome = function () {
                $location.path('home');
                checkWidth();
            }

            angular.element(document).bind('click', function (e) {
                if ($(e.target).hasClass('lx-menu-button') || 
                    $(e.target).parent().hasClass('lx-menu-button') ||
                    $(e.target).hasClass('lx-menu-area') ||
                    $(e.target).parents('.lx-menu-area').length)
                    return;

                $scope.$apply(function () {
                    var menuWasHidden = checkIfSmall();

                    if (menuWasHidden) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                });

            });

            function checkWidth() {
                var width = Math.max($($window).width(), $window.innerWidth);
                $scope.isMenuVisible = (width >= 768);

                broadcastMenuState();
            }

            function checkIfSmall() {
                var menuWasHidden = false;
                var width = Math.max($($window).width(), $window.innerWidth);
                if (width < 768) {

                    if ($scope.isMenuVisible) {
                        $scope.isMenuVisible = false;
                        menuWasHidden = true;

                        broadcastMenuState();
                    }
                }

                return menuWasHidden;
            }

            function broadcastMenuState() {
                $rootScope.$broadcast('lxMessage-showMenuStateChanged',
                    {
                        show: $scope.isMenuVisible,
                    });
            }
        }
    ]);
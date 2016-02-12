(function () {
    'use strict';

    angular.module('app').controller('logController', logController);

    logController.$inject = ['$scope', '$http', '$rootScope'];

    function logController($scope, $http, $rootScope) {
        var vm = $scope;

        $rootScope.appTitle = 'Event Centric Persistence Benchmarks';

        activate();

        function activate() {
            var Model = function () {
                var self = this;

                self.notifications = ko.observableArray();
                self.messages = ko.observableArray();
            };

            Model.prototype = {
                addNotification: function (notification) {
                    var self = this;

                    var messageWasReceived = ko.utils.arrayFirst(self.notifications(),
                        function (receivedNotification) {
                            return receivedNotification.id == notification.id;
                        });

                    if (!messageWasReceived) {
                        self.notifications.push(notification);
                        self.messages.push(notification.message);
                        //self.messages.push(notification.id + '. ' + notification.message);

                        scrollToBottom();

                        // Cleaning...
                        // Removing the first element. http://www.w3schools.com/jsref/jsref_shift.asp
                        // Length of observable arrays: http://stackoverflow.com/questions/9543482/how-to-get-an-observablearrays-length
                        while (self.notifications().length >= 350) {
                            self.notifications.shift();
                        }

                        while (self.messages().length >= 350) {
                            self.messages.shift();
                        }
                    }
                }
            };

            var model = new Model();

            vm.logHub = $.connection.logHub;

            vm.logHub.client.notify = function (notification) {
                model.addNotification(notification);
            };

            vm.logHub.client.newMessage = function (message) {
                //toastr.info(message);
            };

            $.connection.hub.logging = true;
            $.connection.hub.start().done(function () {
                // SignalR is connected...

                vm.logHub.server.sendMessage('SignalR client detected');
            });

            $(function () {
                ko.applyBindings(model);
            });
        }
    }

    function scrollToBottom() {
        $('html, body').animate({ scrollTop: $(document).height() }, 10);
        //$('html, body').animate({ scrollTop: $(document).height() }, 500);
        //window.scrollTo(0, document.body.scrollHeight);
    }
})();
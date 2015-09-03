(function () {
    'use strict';

    angular.module('app').controller('logController', logController);

    logController.$inject = ['$scope', '$http'];

    function logController($scope, $http) {
        var vm = $scope;

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

                    var entry = ko.utils.arrayFirst(self.notifications(),
                        function (receivedNotification) {
                            return receivedNotification.id == notification.id;
                        });

                    if (!entry) {
                        self.notifications.push(notification);
                        self.messages.push(notification.id + '. ' + notification.message);

                        scrollToBottom();

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
                // SingalR is connected...

                vm.logHub.server.sendMessage('Client connected');
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


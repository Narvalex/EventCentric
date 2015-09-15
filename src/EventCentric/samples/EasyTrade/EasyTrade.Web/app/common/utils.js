(function () {
    'use strict';

    angular
        .module('app')
        .factory('utils', utils);

    function utils() {
        var service = {
            disableSubmitButton: disableSubmitButton,
            enableSubmitButton: enableSubmitButton,
            calculateAge: calculateAge,
            animateTransitionTo: animateTransitionTo
        };

        return service;

        // More info: https://www.youtube.com/watch?v=CBQGl6zokMs
        // And also: https://github.com/daneden/animate.css
        function animateTransitionTo(element, animationToRemove, animationToAdd, goToDestination) {
            $(function () {
                $(element).removeClass(animationToRemove).addClass(animationToAdd).one(
                    'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                        //$(this).removeClass('fadeOutRight');
                        goToDestination();
                    })
            });
        }

        function disableSubmitButton() {
            $(function () {
                $("input[type=submit]").attr("disabled", "disabled");
            });
        }

        // source: http://stackoverflow.com/questions/1414365/disable-enable-an-input-with-jquery
        function enableSubmitButton() {
            $(function () {
                $("input[type=submit]").removeAttr('disabled');
            });
        }

        // source: http://stackoverflow.com/questions/4060004/calculate-age-in-javascript
        function calculateAge(dateString) {
            var today = new Date();
            var birthDate = new Date(dateString);
            var age = today.getFullYear() - birthDate.getFullYear();
            var m = today.getMonth() - birthDate.getMonth();
            if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                age--;
            }
            return age;
        }
    }
})();
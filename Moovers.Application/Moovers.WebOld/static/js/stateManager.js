// A queue implementation in local storage -- adds items to Queue, removes items from the beginning of the queue if required
(function() {
    var StateManager = {
        getAll: function(queueName) {
            var currentState = localStorage.getItem(queueName) || "[]";
            return JSON.parse(currentState);
        },

        empty: function(queueName) {
            localStorage[queueName] = JSON.stringify([]);
        },

        store: function(queueName, value, maxToStore) {
            var currentState = this.getAll(queueName);
            currentState.push(value);

            if (maxToStore) {
                currentState = currentState.slice(-maxToStore);
            }

            var stored = false;
            var count = 0;
            while (!stored && count < 10) {
                try {
                    localStorage.setItem(queueName, JSON.stringify(currentState));
                    stored = true;
                }
                catch (e) {
                    currentState.shift();
                    count++;
                }
            }
        }
    };

    window.StateManager = StateManager;
})();
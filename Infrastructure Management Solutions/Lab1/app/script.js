document.addEventListener('DOMContentLoaded', function() {
    const deploymentTime = new Date().toLocaleString();
    document.getElementById('deployment-time').textContent = deploymentTime;

    // Simulate container ID (in a real environment, this could be fetched from the container)
    const containerId = 'demo-' + Math.random().toString(36).substr(2, 9);
    document.getElementById('container-id').textContent = containerId;

    // Function to add updates (can be used to demonstrate changes)
    function addUpdate(message) {
        const updates = document.getElementById('updates');
        const li = document.createElement('li');
        li.textContent = `${new Date().toLocaleString()}: ${message}`;
        updates.insertBefore(li, updates.firstChild);
    }

    // Example of adding an update
    addUpdate('Application started');
});

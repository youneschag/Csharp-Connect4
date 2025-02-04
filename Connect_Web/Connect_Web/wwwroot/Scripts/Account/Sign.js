document.addEventListener('DOMContentLoaded', function () {
    // Validation du formulaire
    document.getElementById('registerForm').addEventListener('submit', function (event) {
        event.preventDefault(); // Empêche la soumission du formulaire pour vérifier la validation

        // Récupérer les valeurs des champs
        var username = document.getElementById('username').value;
        var password = document.getElementById('password').value;
        var confirmPassword = document.getElementById('confirmPassword').value;

        // Si tout est correct, appel à l'API via le contrôleur
        fetch(`/Account/Register?username=${username}&password=${password}&confirmPassword=${confirmPassword}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Vider les champs après le succès
                    document.getElementById('username').value = '';
                    document.getElementById('password').value = '';
                    document.getElementById('confirmPassword').value = '';

                    //Redirection vers la page Connect
                    window.location.href = '/Account/Connect';
                } else {
                    alert("Erreur lors de la création de l'utilisateur: " + data.message);
                }
            })
            .catch(error => {
                alert("Une erreur s'est produite: " + error);
            });
    });
});
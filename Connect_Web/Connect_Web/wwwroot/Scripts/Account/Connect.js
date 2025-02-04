document.addEventListener('DOMContentLoaded', function () {
    let PlayerName = null;
    const gameCode = generateUniqueId();

    function generateUniqueId(length = 9) {
        return Array.from({ length }, () => Math.random().toString(36)[2]).join('');
    }

    // Afficher le conteneur des pop-ups après la connexion
    document.getElementById('connectForm').addEventListener('submit', function (event) {
        event.preventDefault(); // Empêche la soumission classique du formulaire

        // Récupérer les valeurs des champs
        var username = document.getElementById('username').value;
        var password = document.getElementById('password').value;

        // Si les champs sont valides, effectuer l'appel API
        fetch('/Account/Connexion', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username: username, password: password })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Afficher le conteneur parent des pop-ups
                    const popupContainer = document.getElementById('popupContainer');
                    popupContainer.style.display = 'flex';

                    // Afficher le nom d'utilisateur
                    const usernameDisplay = document.getElementById('usernameDisplay');
                    usernameDisplay.textContent = `${data.infos}`;

                    PlayerName = data.infos;
                    sessionStorage.setItem('username', PlayerName);
                } else {
                    alert("Erreur lors de la connexion: " + data.message);
                }
            })
            .catch(error => {
                alert("Une erreur s'est produite: " + error);
            });
    });

    document.getElementById('newGame').addEventListener('click', () => {
        showNewGamePopup();
    });

    // Afficher les options "Nouvelle partie" dans le deuxième pop-up
    function showNewGamePopup() {
        const listPopup = document.getElementById('listPopup');
        const listPopupContent = document.getElementById('listPopupContent');
     
        listPopupContent.innerHTML = `
            <h3 style="font-weight: bold; color: #343a40; margin-bottom: 20px;">Nouvelle Partie</h3>
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
                <div style="padding: 10px; background-color: #007bff; color: white; border-radius: 10px; font-size: 16px; flex: 1; margin-right: 10px;">
                    Hôte : <strong>${PlayerName}</strong>
                </div>
                <div style="padding: 10px; background-color: #e9ecef; color: #343a40; border-radius: 10px; font-size: 16px; flex: 1;">
                    ID de la partie : <strong style="color: #007bff;">${gameCode}</strong>
                </div>
            </div>
            <div style="padding: 10px; margin-bottom: 20px; background-color: #ffc107; color: #343a40; border-radius: 10px; font-size: 16px;">
                Statut : <span id="gameStatus">En attente d'un invité</span>
            </div>
            <div style="display: flex; justify-content: flex-end;">
                <button id="accessGameButton" style="padding: 10px 20px; border: none; border-radius: 30px; background-color: #28a745; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Accéder
                </button>
            </div>
        `;

        document.getElementById('accessGameButton').addEventListener('click', handleGameCreation);

        listPopup.style.display = 'block';

        function handleGameCreation() {
            // Envoyer une requête pour créer une nouvelle partie
            fetch('/Account/GameCreation', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ HostName: PlayerName, GameCode: gameCode }),
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error("Erreur lors de la récupération des données");
                    }
                    return response.json(); // Transformer la réponse en JSON
                })
                .then(data => {
                    if (!data.success) {
                        throw new Error(data.message || "Erreur inconnue");
                    }

                    sessionStorage.setItem('host', PlayerName);

                    window.location.href = `/Account/Game?gameCode=${gameCode}`;
                })
                .catch((error) => {
                    console.error('Une erreur est survenue :', error);
                    alert('Une erreur est survenue : ' + error.message);
                });
        }
    }

    document.getElementById('inviteGame').addEventListener('click', () => {
        showJoinGamePopup();
    });

    function showJoinGamePopup() {
        const listPopup = document.getElementById('listPopup');
        const listPopupContent = document.getElementById('listPopupContent');

        listPopupContent.innerHTML = `
                <h3 style="font-weight: bold; color: #343a40; margin-bottom: 20px;">Rejoindre une Partie</h3>
                <div style="padding: 10px; margin-bottom: 20px; background-color: #007bff; color: white; border-radius: 10px; font-size: 16px;">
                    Guest : <strong>${PlayerName}</strong>
                </div>
                <div style="padding: 10px; margin-bottom: 20px; background-color: #e9ecef; color: #343a40; border-radius: 10px; font-size: 16px;">
                    <input type="text" id="GameCodeInput" placeholder="Code de la partie" style="padding: 10px; width: 100%; border: 1px solid #ccc; border-radius: 30px;" />
                </div>
                <button id="joinGameButton" style="padding: 10px 20px; margin-top: 10px; border: none; border-radius: 30px; background-color: #28a745; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Accéder
                </button>
            </div>
        `;

        document.getElementById('joinGameButton').addEventListener('click', handleGameJoin);

        listPopup.style.display = 'block';

        function handleGameJoin() {
            const gameCode = document.getElementById('GameCodeInput').value.trim();

            // Envoyer une requête pour créer une nouvelle partie
            fetch(`/Account/GameJoin?GuestName=${PlayerName}&GameCode=${gameCode}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error("Erreur lors de la récupération des données");
                    }
                    return response.json(); // Transformer la réponse en JSON
                })
                .then(data => {
                    if (!data.success) {
                        throw new Error(data.message || "Le code de la partie n'appartient à aucune partie existante");
                    }

                    sessionStorage.setItem('guest', PlayerName);

                    window.location.href = `/Account/Game?gameCode=${gameCode}`;
                })
                .catch((error) => {
                    console.error('Une erreur est survenue :', error);
                    alert('Une erreur est survenue : ' + error.message);
                });
        }
    }

    function showListPopup(type) {
        const listPopup = document.getElementById('listPopup');
        const listPopupContent = document.getElementById('listPopupContent');
        const username = document.getElementById('usernameDisplay').textContent.trim(); // Récupérer le nom de l'utilisateur

        // URL de l'API en fonction du type
        const apiUrl = type === 'Hôte'
            ? GetHostGamesUrl + username
            : GetGuestGamesUrl + username;

        // Effectuer la requête API
        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Erreur lors de la récupération des données");
                }
                return response.json(); // Transformer la réponse en JSON
            })
            .then(data => {
                if (!data.success) {
                    throw new Error(data.message || "Erreur inconnue");
                }

                // Désérialiser les parties si elles sont renvoyées sous forme de chaîne JSON
                const games = JSON.parse(data.games);

                // Si aucune partie n'est trouvée
                if (games.length === 0) {
                    listPopupContent.innerHTML = `
                    <p id="listTitle" style="font-weight: bold;">Liste des parties ${type}</p>
                    <p style="text-align: center; color: gray;">Aucune partie trouvée pour l'instant.</p>
                    `;
                    listPopup.style.display = 'block';
                    return;
                }

                // Remplacer le contenu avec un tableau
                listPopupContent.innerHTML = `
                <p id="listTitle" style="font-weight: bold;">Liste des parties ${type}</p>
                <table id="listTable" style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                    <thead>
                        <tr>
                            <th></th>
                            <th>Partie</th>
                            <th>Statut</th>
                            <th>Date de création</th>
                        </tr>
                    </thead>
                    <tbody id="listTableBody">
                        <!-- Les lignes des parties seront insérées ici dynamiquement -->
                    </tbody>
                </table>
                `;

                // Ajouter les données au tableau
                const listTableBody = document.getElementById('listTableBody');
                listTableBody.innerHTML = ''; // Réinitialiser le tableau

                games.forEach((item, index) => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${index + 1}</td> <!-- Numérotation des lignes -->
                        <td>
                            ${item.winner ?
                            `<span class="${item.winner === item.hostName ? 'winner' : 'loser'}">${item.hostName}</span> /
                            <span class="${item.winner === item.guestName ? 'winner' : 'loser'}">${item.guestName}</span>`
                            : `${item.hostName} / ${item.guestName}`
                            }
                        </td>
                        <td>${item.statusText}</td>
                        <td>${item.creationDate}</td>
                    `;

                    // Ajouter l'événement de clic pour les parties
                    row.addEventListener('click', () => {
                        window.location.href = `/Account/Game?gameCode=${item.gameCode}`;
                    });

                    listTableBody.appendChild(row);
                });

                // Afficher le deuxième pop-up
                listPopup.style.display = 'block';
            })
            .catch(error => {
                console.error('Erreur lors de la récupération des parties :', error);
                alert('Une erreur est survenue lors de la récupération des parties.');
            });
    }

    // Ajouter les gestionnaires pour les boutons
    document.getElementById('hostButton').addEventListener('click', () => {
        showListPopup('Hôte');
    });

    document.getElementById('inviteButton').addEventListener('click', () => {
        showListPopup('Invité');
    });
});
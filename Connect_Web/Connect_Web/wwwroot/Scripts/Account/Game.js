$(document).ready(async function () {
    const rows = 6;
    const cols = 7;
    const gridContainer = $('.grid-container');

    let gameActive = true;
    let gameStarted = false;
    let currentTurn;

    let grid = [];
    let gameCode = null;
    let host = null;
    let gameId;
    let GameStatus;
    let guest = null;
    let username = sessionStorage.getItem('username');
    let GameWinner = null;

    // Extraire les paramètres de l'URL
    const urlParams = new URLSearchParams(window.location.search);
    gameCode = urlParams.get('gameCode');

    const gameToken = generateUniqueId();

    function generateUniqueId(length = 9) {
        return Array.from({ length }, () => Math.random().toString(36)[2]).join('');
    }

    // Appeler la fonction pour charger les mouvements après avoir récupéré gameId
    initializeGrid();

    // Configurer le rafraîchissement automatique de l'état du jeu toutes les 1 secondes
    setInterval(async () => {
        // Charger les données du jeu
        await loadGameData();
        await processGameMoves(gameId);

        // Mettre à jour les colonnes des joueurs avec les noms récupérés
        $('.player-column .player-name').eq(0).text(host || "Hôte"); // Nom de l'hôte
        $('.player-column .player-name').eq(1).text(guest || "En attente d'un invité"); // Nom de l'invité

        fetchGames(GetHostGamesUrl + username, 'hostGamesList');
        fetchGames(GetGuestGamesUrl + username, 'guestGamesList');
        updateActiveGameList();

        if (GameWinner !== null) {
            const gameResultMessage = document.getElementById('gameResultMessage');
            const resultText = document.getElementById('resultText');

            if (GameWinner === 'Both') {
                resultText.textContent = "Match nul ! Bien joué à tous les deux 🎉";
            } else {
                resultText.textContent = `${GameWinner} a gagné la partie ! Félicitations 🎉`;
            }

            gameResultMessage.style.display = 'flex';
        }
    }, 1000);

    async function loadGameData() {
        try {
            // Récupérer les données de la partie
            const response = await fetch(`/Account/InfosGame?GameCode=${gameCode}&username=${username}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
            });

            if (!response.ok) {
                throw new Error("Erreur lors de la récupération des données");
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || "Erreur inconnue");
            }

            // Désérialisez `data.game` si nécessaire
            const gameArray = JSON.parse(data.game); // Désérialisation
            const game = gameArray[0]; // Récupérez le premier objet du tableau
            gameId = game.id;
            host = game.hostName;
            guest = game.guestName;
            currentTurn = game.currentTurn;
            GameStatus = game.status;
            GameWinner = game.winner;
        } catch (error) {
            console.error('Erreur lors de la mise à jour du statut :', error);
        }
    }

    // Initialisez la grille
    function initializeGrid(gameId) {
        for (let y = 0; y < rows; y++) {
            grid[y] = [];
            for (let x = 0; x < cols; x++) {
                const cell = $('<div>')
                    .addClass('grid-item')
                    .append($('<div>').addClass('circle').attr('data-x', x).attr('data-y', y));
                grid[y][x] = cell;
                gridContainer.append(cell);
            }
        }
    }

    // Fonction pour charger les mouvements existants
    async function processGameMoves(gameId) {
        try {
            const response = await fetch(`/Account/GameMoves/?GameId=${gameId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
            });

            if (!response.ok) {
                throw new Error("Erreur lors de la récupération des mouvements");
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || "Erreur inconnue");
            }

            // Désérialiser la chaîne JSON contenue dans data.moves
            const moves = JSON.parse(data.moves);

            // Réinitialiser les listes de mouvements avant d'ajouter les nouveaux
            $('#moves-yellow').empty();
            $('#moves-red').empty();

            moves.forEach(move => {
                const cell = grid[move.y][move.x]; // Récupérer la cellule dans `grid`
                cell.find('.circle').addClass(`token-${move.color}`);

                addMove(move.x, move.y, move.color);
            });
        } catch (error) {
            console.error('Erreur lors de la récupération des mouvements :', error);
        }
    }

    // Mettre à jour la liste des parties actives avec l'état initial
    function updateActiveGameList() {
        fetch(`/Account/GetActiveGames?username=${username}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Erreur lors de la récupération des parties actives");
                }
                return response.json();
            })
            .then(data => {
                // Vérifiez si la réponse a réussi
                if (!data.success) {
                    throw new Error(data.message || "Erreur inconnue");
                }

                // Désérialiser les parties si elles sont renvoyées sous forme de chaîne JSON
                const games = JSON.parse(data.games);

                // Supprimez les anciens éléments de la liste pour la réinitialiser
                const list = $('#activeGamesList');
                list.empty();

                // Parcourez chaque partie active
                games.forEach(game => {
                    // Déterminez à qui c'est de jouer
                    const turnText = game.currentTurn === 'red' ? game.guestName : game.hostName;

                    // Créez un nouvel élément de liste
                    const listItem = $(`<li data-game-code="${game.gameCode}">
                        Partie: ${game.gameCode} - À ${turnText} de jouer
                    </li>`);

                    // Ajoutez l'élément de liste
                    list.append(listItem);
                });
            })
            .catch(error => {
                console.error('Erreur lors de la mise à jour de la liste des parties actives :', error);
            });
    }

    // Fonction pour gérer le clic sur une cellule
    $('.circle').click(async function () {
        // Vérifier si le jeu est actif
        if (!gameActive || GameStatus == 2) return;

        const x = $(this).data('x');

        // Démarrer le chronomètre au premier jeton
        if (!gameStarted) {
            gameStarted = true;

            // Mettre à jour le statut de la partie à "En cours"
            fetch(`/Account/UpdateGameStatus?Id=${gameId}&status=${1}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
            })
                .then(response => response.json())
                .then(data => {
                    if (!data.success) {
                        alert('Erreur lors de la mise à jour de la partie');
                    }
                })
                .catch(error => {
                    console.error('Erreur lors de la mise à jour du statut :', error);
                });
        }

        // Envoyer une requête pour ajouter un jeton
        try {
            const response = await fetch('/Account/AddToken', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    GameId: gameId,
                    X: x,
                    color: currentTurn,
                    PlayerName: username,
                }),
            });

            const data = await response.json();

            if (response.ok && data.result) {
                // Désérialiser la chaîne JSON contenue dans data.result
                const result = JSON.parse(data.result);

                // Ajouter le jeton sur la grille
                grid[result.y][x].find('.circle').addClass(`token-${currentTurn}`);

                // Passer au tour suivant
                currentTurn = result.nextTurn;

                // Vérifier la victoire ou le match nul via le backend
                if (result.message === "Victoire !") {
                    finishingGame(result.winner);
                } else if (result.message === "Match nul !") {
                    finishingGame("Both"); 
                }
            } else {
                alert(data.message || "Erreur lors de l'ajout du jeton");
            }
        } catch (error) {
            console.error("Erreur lors de l'fjfnr du jeton :", error);
        }
    });

    function finishingGame(winner) {
        gameActive = false;

        // Mettre à jour le statut de la partie à "terminée"
        fetch(`/Account/UpdateGameStatus?Id=${gameId}&status=${2}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    declareWinner(winner);
                }
            })
            .catch(error => {
                console.error('Erreur lors de la mise à jour du statut :', error);
            });
    }

    // Fonction pour ajouter un mouvement dans le conteneur correspondant
    function addMove(x, y, color) {
        const moveContainer = color === 'yellow' ? $('#moves-yellow') : $('#moves-red');
        const move = $('<div>')
            .addClass('move')
            .addClass(`move-${color}`)
            .text(`Colonne: ${x}, Ligne: ${y}`); 
        moveContainer.append(move);
    }

    function declareWinner(winner) {
        fetch(`/Account/DeclareWinner?Id=${gameId}&Winner=${winner}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        })
            .then(response => response.json())
            .then(data => {
                if (!data.success) {
                    alert('Erreur lors de la mise à jour du statut de la partie.');
                }
            })
            .catch(error => {
                console.error('Erreur lors de la mise à jour du statut :', error);
            });
    }

    // Gestion des listes déroulantes
    $('.list-header').each(function () {
        const listId = $(this).data('list');
        const list = $(`#${listId}`);
        const arrow = $(this).find('.arrow');

        // Si la liste a la classe `open`, assurez-vous qu'elle est affichée
        if ($(this).hasClass('open')) {
            list.addClass('open').css('display', 'block');
            arrow.addClass('open').html('&#9660;'); // Flèche vers le bas
        } else {
            list.removeClass('open').css('display', 'none');
            arrow.removeClass('open').html('&#9650;'); // Flèche vers le haut
        }
    });

    // Alterner l'ouverture/fermeture des listes sur clic
    $('.list-header').click(function () {
        const listId = $(this).data('list');
        const list = $(`#${listId}`);
        const arrow = $(this).find('.arrow');

        // Alterner entre ouvrir et fermer
        if (list.hasClass('open')) {
            list.removeClass('open').css('display', 'none');
            arrow.removeClass('open').html('&#9650;'); // Flèche vers le haut
        } else {
            list.addClass('open').css('display', 'block');
            arrow.addClass('open').html('&#9660;'); // Flèche vers le bas
        }
    });

    // Fonction pour récupérer les données depuis l'API
    function fetchGames(apiUrl, listId) {
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

                // Remplir la liste avec les données
                renderGameList(listId, games);
            })
            .catch(error => {
                console.error(`Erreur lors de la récupération des parties pour ${listId} :`, error);
                alert('Une erreur est survenue lors de la récupération des parties.');
            });
    }

    // Fonction pour remplir une liste
    function renderGameList(listId, games) {
        const list = $(`#${listId}`);
        list.empty(); // Réinitialiser la liste
        if (games.length === 0) {
            list.append('<li>Aucune partie trouvée</li>');
            return;
        }
        games.forEach(game => {
            const listItem = $(`<li>${game.hostName} / ${game.guestName} - ${game.statusText}</li>`);
            listItem.on('click', () => {
                sessionStorage.setItem('guest', game.guestName);
                sessionStorage.setItem('host', game.hostName);
                sessionStorage.setItem('username', username);

                window.location.href = `/Account/Game?gameCode=${game.gameCode}`;
            });
            list.append(listItem);
        });
    }

    document.querySelectorAll('.change-avatar-icon').forEach((icon, index) => {
        // Identifier la liste des avatars correspondante
        const avatarList = index === 0 ? document.getElementById('avatarListFalse') : document.getElementById('avatarListTrue');
        const defaultAvatar = index === 0
            ? document.querySelector('.player-column:nth-child(1) .player-photo img')
            : document.querySelector('.player-column:nth-child(3) .player-photo img');

        // Afficher/Masquer la liste des avatars au clic sur l'icône
        icon.addEventListener('click', (event) => {
            event.stopPropagation(); // Empêcher le clic de se propager
            const isListVisible = avatarList.style.display === 'block';

            // Masquer toutes les autres listes
            document.querySelectorAll('#avatarListFalse, #avatarListTrue').forEach(list => {
                list.style.display = 'none';
            });

            // Alterner la visibilité de la liste actuelle
            avatarList.style.display = isListVisible ? 'none' : 'block';
        });

        // Modifier l'avatar au clic sur une option
        avatarList.querySelectorAll('.avatar-option').forEach(option => {
            option.addEventListener('click', (event) => {
                const newAvatar = event.target.src;
                defaultAvatar.src = newAvatar; // Changer l'avatar affiché
                avatarList.style.display = 'none'; // Masquer la liste après sélection
            });
        });
    });

    // Masquer les listes si on clique en dehors
    document.addEventListener('click', (event) => {
        document.querySelectorAll('#avatarListFalse, #avatarListTrue').forEach(list => {
            list.style.display = 'none';
        });
    });

    document.getElementById('newGame').addEventListener('click', () => {
        document.getElementById('popupContainer').style.display = 'flex';
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
                    Hôte : <strong>${username}</strong>
                </div>
                <div style="padding: 10px; background-color: #e9ecef; color: #343a40; border-radius: 10px; font-size: 16px; flex: 1;">
                    ID de la partie : <strong style="color: #007bff;">${gameToken}</strong>
                </div>
            </div>
            <div style="padding: 10px; margin-bottom: 20px; background-color: #ffc107; color: #343a40; border-radius: 10px; font-size: 16px;">
                Statut : <span id="gameStatus">En attente d'un invité</span>
            </div>
            <div style="display: flex; justify-content: center; gap: 10px; margin-top: 20px;">
                <button id="closePopup" style="padding: 10px 20px; border: none; background-color: #FF5900; border-radius: 30px; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Retour
                </button>
                <button id="accessGameButton" style="padding: 10px 20px; border: none; background-color: #00FF64; border-radius: 30px; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Accéder
                </button>
            </div>
        `;

        document.getElementById('accessGameButton').addEventListener('click', handleGameCreation);
        document.getElementById('closePopup').addEventListener('click', closePopup);
        
        listPopup.style.display = 'block';

        function closePopup() {
            document.getElementById('popupContainer').style.display = 'none';
        }

        function handleGameCreation() {
            // Envoyer une requête pour créer une nouvelle partie
            fetch('/Account/GameCreation', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ HostName: username, GameCode: gameToken }),
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

                    sessionStorage.setItem('host', username);

                    window.location.href = `/Account/Game?gameCode=${gameToken}`;
                })
                .catch((error) => {
                    console.error('Une erreur est survenue :', error);
                    alert('Une erreur est survenue : ' + error.message);
                });
        }
    }

    document.getElementById('joinGame').addEventListener('click', () => {
        document.getElementById('popupContainer').style.display = 'flex';
        showJoinGamePopup();
    });

    function showJoinGamePopup() {
        const listPopup = document.getElementById('listPopup');
        const listPopupContent = document.getElementById('listPopupContent');

        listPopupContent.innerHTML = `
            <h3 style="font-weight: bold; color: #343a40; margin-bottom: 20px;">Rejoindre une Partie</h3>
            <div style="padding: 10px; margin-bottom: 20px; background-color: #007bff; color: white; border-radius: 10px; font-size: 16px;">
                Guest : <strong>${username}</strong>
            </div>
            <div style="padding: 10px; margin-bottom: 20px; background-color: #e9ecef; color: #343a40; border-radius: 10px; font-size: 16px;">
                <input type="text" id="GameCodeInput" placeholder="Code de la partie" style="padding: 10px; width: 100%; border: 1px solid #ccc; border-radius: 30px;" />
            </div>
            <div style="display: flex; justify-content: center; gap: 10px; margin-top: 20px;">
                <button id="closePopup" style="padding: 10px 20px; border: none; border-radius: 30px; background-color: #FF5900; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Retour
                </button>
                <button id="joinGameButton" style="padding: 10px 20px; border: none; border-radius: 30px; background-color: #28a745; color: white; cursor: pointer; font-size: 16px; transition: background-color 0.3s ease;">
                    Rejoindre
                </button>
            </div>
        `;

        document.getElementById('joinGameButton').addEventListener('click', handleGameJoin);
        document.getElementById('closePopup').addEventListener('click', closePopup);

        listPopup.style.display = 'block';

        function closePopup() {
            document.getElementById('popupContainer').style.display = 'none';
        }

        function handleGameJoin() {
            const gameJoinCode = document.getElementById('GameCodeInput').value.trim();

            // Envoyer une requête pour créer une nouvelle partie
            fetch(`/Account/GameJoin?GuestName=${username}&GameCode=${gameJoinCode}`, {
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

                    sessionStorage.setItem('guest', username);

                    window.location.href = `/Account/Game?gameCode=${gameJoinCode}`;
                })
                .catch((error) => {
                    console.error('Une erreur est survenue :', error);
                    alert('Une erreur est survenue : ' + error.message);
                });
        }
    }
});
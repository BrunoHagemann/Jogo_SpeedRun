# Returns of the Obima Pyramid — FPS3000

Projeto Unity de ação em primeira pessoa, com armas, inimigos, objetivos por fase e cronômetro para speedrun. O jogador percorre fases com temas e cenários diferentes; concluir os objetivos leva à tela de vitória e morrer leva à tela de derrota.

## Tecnologias e configurações principais

- **Unity:** 6000.3.24f1.
- **Renderização:** Universal Render Pipeline (URP) 17.3.0.
- **Entrada:** Input System 1.20.0, com ações de movimento, olhar, pulo, corrida, agachamento, disparo, mira, recarga, troca de arma e dash em `Assets/FPS/InputSystem_Actions.inputactions`.
- **Pacotes relevantes:** AI Navigation 2.0.14, ProBuilder 6.1.2 e Timeline 1.8.13.
- **Cena inicial no build:** `Assets/FPS/Scenes/IntroMenu.unity`. A ordem completa está em `ProjectSettings/EditorBuildSettings.asset`.
- **Resolução padrão:** 960 × 540; qualidade padrão usa 4 luzes por pixel, sem antialiasing e VSync desativado.

## Organização do projeto

- `Assets/FPS/` — núcleo jogável: scripts, cenas, prefabs, interface e recursos do jogo.
- `Assets/AGENTS/` — documentação e orientações para trabalho no repositório.
- `Assets/POLYGON city pack/`, `Assets/Free LowPoly SciFi Pack/` e `Assets/FreeAnimatedSpaceMan/` — recursos de cenários e personagem.
- `Assets/Low Poly Weapons VOL.1/`, `Assets/Kevin Iglesias/` e `Assets/AttackBot/` — armas, animações e modelos de inimigos.
- `Assets/Rendering/` — configurações e perfis de renderização.
- `Packages/manifest.json` — dependências Unity do projeto.
- `ProjectSettings/` — versão, renderização, entrada, qualidade e lista de cenas do build.

## Scripts principais

Os scripts de jogo ficam em `Assets/FPS/Scripts/`, agrupados por responsabilidade:

- **Gameplay:** `PlayerCharacterController` controla o movimento em primeira pessoa; `PlayerInputHandler` lê as ações; `PlayerWeaponsManager`, `WeaponPickup` e scripts de projéteis/pickups cuidam de armas, munição, vida e jetpack.
- **Objetivos e fluxo:** `ObjectiveReachPoint`, `ObjectivePickupItem` e `ObjectiveKillEnemies` implementam objetivos; `ObjectiveManager` detecta quando todos foram concluídos; `GameFlowManager` conduz vitória/derrota e troca de cena.
- **Inimigos:** `EnemyController`, `EnemyManager`, `DetectionModule`, `NavigationModule`, `EnemyZombieMelee`, `EnemyTurret` e `EnemyMobile` implementam detecção, navegação e comportamentos.
- **Interface:** scripts em `Scripts/UI/` atualizam HUD de vida, munição, objetivos, notificações, bússola e menus.
- **Speedrun:** `Timer` mede o tempo da fase, salva o último tempo e o recorde em `PlayerPrefs`; `VictoryTimeDisplay` mostra os tempos na tela de vitória.
- **Fases e utilitários:** `LevelSelector` carrega uma cena pelo nome; os scripts em `Scripts/Game/` apoiam eventos, áudio, dano, atores e gerenciamento compartilhado.

## Cenas incluídas no build

As cenas abaixo estão habilitadas em `EditorBuildSettings.asset`, nesta ordem:

1. `IntroMenu` — menu inicial.
2. `WinScene` e `LoseScene` — telas de resultado.
3. `WinScene 1`, `WinScene 2` e `WinScene 3` — variações de cenas de vitória.
4. `SciFi`, `4Scene`, `3Scene`, `Cidade`, `MainScene` e `SecondaryScene` — cenas de jogo/fases.
5. `MenuDeFases` — seleção de fases.

Há também cenas de demonstração e conteúdo de pacotes no projeto; elas não fazem parte da lista de build acima.

## Prefabs de jogo

Os prefabs reutilizáveis do jogo estão em `Assets/FPS/Prefabs/`:

- `Player.prefab` e `GameManager.prefab` — jogador e gerenciamento da partida.
- `Enemies/` — zumbi, torre e inimigo móvel.
- `Weapons/`, `Projectiles/` e `Pickups/` — armas, projéteis, munições e itens de vida/jetpack.
- `ObjectiveKillEnemies.prefab`, `ObjectivePickupItem.prefab` e `ObjectiveReachPoint.prefab` — objetivos configuráveis para montar fases.
- `UI/` e `VFX/` — HUD, menus, mensagens e efeitos visuais.
- `Level/` — peças modulares de cenário, incluindo salas, paredes, pisos, rampas, escadas e seções da fase principal.

Os pacotes em outras pastas de `Assets/` também incluem seus próprios prefabs e cenas de demonstração.

## Fluxo básico de uma partida

O menu carrega a fase escolhida. Durante a partida, o jogador completa objetivos e pode coletar armas e itens. Quando todos os objetivos terminam, `GameFlowManager` encerra a fase como vitória; a morte do jogador encerra como derrota. O cronômetro grava `LastSpeedrunTime` e atualiza `BestSpeedrunTime` na vitória, e a tela de vitória lê esses valores.

Para adicionar uma cena ao jogo inicializado pelo build, inclua-a na lista de cenas do Build Settings. Para criar uma fase, reutilize os prefabs de `Assets/FPS/Prefabs/Level/`, configure os objetivos e confira as referências de interface e fluxo na cena.

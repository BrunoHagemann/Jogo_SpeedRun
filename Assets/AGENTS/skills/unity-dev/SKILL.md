---
name: unity-dev
description: Implemente ou altere funcionalidades de jogos Unity neste projeto quando o pedido exigir desenvolvimento de scripts, cenas, prefabs ou gameplay.
---

# [IMPORTANTE] USE ESTA SKILL PARA DESENVOLVER NO PROJETO UNITY

Esta skill orienta a implementação de funcionalidades Unity em C#, cenas e prefabs, respeitando a estrutura e os recursos já usados pelo projeto.

## Regra de Ativação (Gating Rule)

Ative esta skill quando o usuário pedir para criar, implementar, ampliar ou alterar uma funcionalidade do jogo. Se o pedido for apenas uma revisão ou um erro já existente, use a skill específica de revisão ou depuração.

Antes de implementar:

- Leia as orientações aplicáveis em `../../AGENTS.md` e inspecione os scripts, cenas, prefabs e configurações diretamente relacionados ao pedido.
- Confirme o comportamento esperado e identifique as referências serializadas, eventos, ações de entrada e cenas que a mudança precisa preservar.
- Use a versão e os pacotes existentes no projeto; consulte documentação Unity oficial quando APIs ou comportamentos não estiverem claros.

Durante a implementação:

- Faça a menor alteração que entregue o resultado solicitado e preserve os padrões arquiteturais existentes.
- Para alterações em cena ou prefab, mantenha referências do Inspector, hierarquia e componentes necessários funcionando.
- Não altere arquivos `.meta` manualmente, nem `ProjectSettings/` ou dependências em `Packages/` sem autorização explícita.
- Explique cada escolha em uma linha objetiva, incluindo o motivo prático.
- Siga as regras de conferência e execução de testes definidas em `../../AGENTS.md`.

Conclua quando a funcionalidade pedida estiver implementada, os vínculos afetados tiverem sido conferidos e o resultado tiver sido resumido de forma breve.

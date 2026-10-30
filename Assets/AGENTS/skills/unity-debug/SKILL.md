---
name: unity-debug
description: Diagnostique e corrija erros, crashes ou comportamentos incorretos em scripts e projetos Unity com base em evidências.
---

# [IMPORTANTE] USE ESTA SKILL PARA RESOLVER ERROS E PROBLEMAS UNITY

Esta skill investiga a causa de um erro Unity ou de um comportamento incorreto e orienta uma correção pequena, verificável e compatível com o projeto.

## Regra de Ativação (Gating Rule)

Ative esta skill quando o usuário trouxer uma mensagem da Console, erro de compilação, crash, falha de cena ou comportamento que não corresponde ao esperado. Se o pedido for uma revisão geral sem falha específica, use a skill de revisão; para uma funcionalidade nova, use a skill de desenvolvimento.

Ao investigar e corrigir:

- Leia `../../AGENTS.md`; reúna evidências no código, referências serializadas, configuração relevante, stack trace ou passos de reprodução disponíveis.
- Rastreie o fluxo até a causa provável e diferencie fatos confirmados de hipóteses; peça informação apenas se algo essencial não puder ser encontrado ou inferido.
- Aplique a correção mínima que resolve a causa, preservando compatibilidade com cenas, prefabs, eventos e ações existentes.
- Não esconda o erro com tratamento genérico, não faça refatorações sem relação e não altere `ProjectSettings/` ou dependências sem autorização explícita.
- Explique cada escolha em uma linha objetiva e informe como a correção foi conferida; siga as regras de testes de `../../AGENTS.md`.

Conclua quando a causa e a correção estiverem explicadas, as partes afetadas tiverem sido conferidas e limitações restantes estiverem descritas com clareza.

---
name: unity-review
description: Revise código Unity/C# quando o usuário pedir análise de qualidade, correção, segurança, desempenho ou riscos de regressão.
---

# [IMPORTANTE] USE ESTA SKILL PARA REVISAR CÓDIGO UNITY

Esta skill conduz uma revisão técnica focada em defeitos concretos e riscos no comportamento do jogo, com achados que o usuário consiga localizar e avaliar.

## Regra de Ativação (Gating Rule)

Ative esta skill quando o usuário pedir revisão, auditoria ou análise de código Unity, ou quando encontrar problemas fizer parte explícita do pedido. Para implementar uma funcionalidade nova, use a skill de desenvolvimento; para diagnosticar um erro específico, use a skill de depuração.

Na revisão:

- Leia `../../AGENTS.md` e inspecione o código e os assets relacionados ao escopo solicitado; amplie a busca somente se as referências mostrarem dependências relevantes.
- Priorize defeitos que possam causar falha, comportamento incorreto, perda de referências serializadas, vulnerabilidade ou impacto mensurável no desempenho.
- Apresente primeiro os achados, ordenados por gravidade, com arquivo e linha, condição de ocorrência e consequência observável.
- Separe fatos confirmados de hipóteses e não registre preferências de estilo como defeitos.
- Não altere código durante uma revisão somente de análise; corrija achados quando o usuário também tiver pedido a correção.
- Explique cada recomendação em uma linha objetiva, com o motivo prático.
- Rode testes ou verificações apenas conforme as orientações de `../../AGENTS.md` e o pedido do usuário.

Conclua quando o escopo estiver revisado, os achados estiverem claros e nenhuma afirmação depender de suposição apresentada como fato.

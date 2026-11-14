# Roteiro Oficial de Testes de Funções
# Mod: Reborn: Evolving From Nothing — Magic & Soul Matrix

Este documento contém o checklist prático e metódico para validar cada mecânica implementada no mod.

---

## Índice de Testes
1. [Teste 1: Interface da Aba "Alma" (Unicidade e Estabilidade)](#teste-1-interface-da-aba-alma)
2. [Teste 2: Matriz da Alma & Auto-Despertar na Classe Mortal](#teste-2-matriz-da-alma--auto-despertar)
3. [Teste 3: Cultivo Contínuo — Meditar na Matriz](#teste-3-cultivo-contínuo--meditar-na-matriz)
4. [Teste 4: Ataque Espiritual — Disparo da Alma (Soul Bolt)](#teste-4-ataque-espiritual--disparo-da-alma)
5. [Teste 5: Obtenção e Fusão da Pérola Mágica](#teste-5-obtenção-e-fusão-da-pérola-mágica)
6. [Teste 6: Absorção de Cadáveres & Registro de Linhagens](#teste-6-absorção-de-cadáveres--registro-de-linhagens)
7. [Teste 7: Metamorfose, Vácuo Espiritual e Restrição de Bancada](#teste-7-metamorfose-vácuo-espiritual-e-restrição-de-bancada)
8. [Teste 8: Maturação da Besta e Batismo das Leis (Classe Terra)](#teste-8-maturação-da-besta-e-batismo-das-leis)
9. [Teste 9: Avanço para Classe Céu & Feitiços Celestiais](#teste-9-avanço-para-classe-céu--feitiços-celestiais)
10. [Teste 10: Atributos Primários Iniciais (Cálculo Dinâmico por Vocação Vanilla)](#teste-10-atributos-primários-iniciais)
11. [Teste 11: Progressão de Níveis, Curva Exponencial & Distribuição de Pontos](#teste-11-progressão-de-níveis-e-distribuição-de-pontos)
12. [Teste 12: Barra de Vigor (SP) e Impacto dos Atributos em Combate](#teste-12-barra-de-vigor-e-impacto-dos-atributos)
13. [Teste 13: Sub-Aba Grimório de Técnicas, Maestria e Foco na Meditação](#teste-13-sub-aba-grimório-de-técnicas)
14. [Teste 14: As 10 Novas Técnicas de Cultivo em Ação](#teste-14-as-10-novas-técnicas-de-cultivo)
15. [Teste 15: Escudo Psiônico — Absorção Unilateral & Disparo de Dentro](#teste-15-escudo-psiônico--absorção-unilateral)
16. [Teste 16: Projéteis Balísticos Voadores & Bloqueio Total de Efeitos por Escudo](#teste-16-projéteis-balísticos-voadores)

---

### Teste 1: Interface da Aba "Alma"
* **Objetivo**: Garantir que a aba esteja visível em todos os colonos e sem duplicações.
* **Passo a passo**:
  1. No jogo, clique com o botão esquerdo em qualquer colono livre da sua colônia.
  2. Olhe para a barra de abas no painel inferior de inspeção (onde ficam *Social*, *Saúde*, *Equipamento*, etc.).
* **Critérios de Aceite**:
  * [ ] O botão **"Alma"** aparece exatamente **uma única vez** (não duplicado).
  * [ ] Clicar no botão abre a janela da Matriz sem travar o jogo ou lançar erros no log.

---

### Teste 2: Matriz da Alma & Auto-Despertar
* **Objetivo**: Garantir que qualquer colono humanoide tenha sua Matriz da Alma inicializada.
* **Passo a passo**:
  1. Com a aba "Alma" aberta no colono, verifique os indicadores.
  2. Selecione o colono e recrute-o (*Draft*, tecla `R`).
* **Critérios de Aceite**:
  * [ ] O título exibe **★ Classe Mortal (Mortal Class)**.
  * [ ] A barra de energia exibe `100 / 100`.
  * [ ] A barra de compressão exibe `0 / 200 XP (0%)`.
  * [ ] Quando recrutado (*Drafted*), uma barra azul de energia sutil é desenhada sobre a cabeça do peão.

---

### Teste 3: Cultivo Contínuo — Meditar na Matriz
* **Objetivo**: Verificar a circulação passiva de energia e compressão da alma.
* **Passo a passo**:
  1. Selecione o colono e clique no comando **"Meditar na Matriz"** (ícone da joia mágica).
  2. Acelere o tempo do jogo na velocidade 2 ou 3.
* **Critérios de Aceite**:
  * [ ] O colono senta no chão e seu status no painel inferior passa a ser *"meditando na Matriz da Alma"*.
  * [ ] Círculos psíquicos de partículas se manifestam periodicamente ao redor do colono.
  * [ ] A cada hora de jogo, a energia recupera `+100` e a compressão acumula `+100 XP`.
  * [ ] O jogador pode interromper o colono a qualquer momento dando outra ordem.

---

### Teste 4: Ataque Espiritual — Disparo da Alma (Soul Bolt)
* **Objetivo**: Testar o feitiço ofensivo básico da Classe Mortal.
* **Passo a passo**:
  1. Recrute o colono (*Draft*).
  2. Na barra de comandos, clique na habilidade **"Disparo da Alma"** (alcance de 22 tiles).
  3. Aponte e clique sobre um alvo hostil (ou criatura para teste).
  4. Repita o disparo até que a energia caia para menos de 20 pontos.
* **Critérios de Aceite**:
  * [ ] O feitiço consome exatamente `20` pontos de Energia da Alma.
  * [ ] Clarões espirituais atingem o alvo, causando `25` de dano direto.
  * [ ] Um texto flutuante *"GOLPE ESPIRITUAL!"* aparece sobre o alvo.
  * [ ] O colono ganha `+8 XP` de compressão por disparo lançado.
  * [ ] Ao tentar disparar com menos de 20 de energia, uma mensagem vermelha informa *"Energia espiritual insuficiente!"*.

---

### Teste 5: Obtenção e Fusão da Pérola Mágica
* **Objetivo**: Validar a transição de um humano comum para o Reencarnado.
* **Passo a passo**:
  1. *Opção A*: Crie um colono com o Trait `Portador da Pérola Mágica` na criação de jogo/Character Editor.
  2. *Opção B (Via Artefato)*: No Dev Mode, use `Spawn Thing` $\rightarrow$ `REM_Item_MagicPearl` (ou fabrique na bancada).
  3. Com um colono comum selecionado, ordene-o a usar o item **"Fundir Pérola Mágica à Alma"**.
* **Critérios de Aceite**:
  * [ ] O item é consumido em um clarão estelar.
  * [ ] Surge uma carta comemorativa informando que a Pérola quebrou as leis biológicas do universo.
  * [ ] O colono adquire o Trait `Portador da Pérola Mágica`.
  * [ ] A aba "Alma" expande seu tamanho vertical e exibe a seção dourada **"★ A Pérola Mágica: Galeria de Linhagens Catalogadas"**.

---

### Teste 6: Absorção de Cadáveres & Registro de Linhagens
* **Objetivo**: Validar a exclusividade da absorção e o sistema de registro genético.
* **Passo a passo**:
  1. Verifique um colono comum: ele **NÃO** deve ter a habilidade "Absorver Alma de Cadáver".
  2. Selecione o **Portador da Pérola Mágica**: ele **DEVE** ter a habilidade "Absorver Alma de Cadáver".
  3. Abata qualquer criatura no mapa (ex: Muffalo, Lobo, Megasscaravelho ou Coelho).
  4. Use a habilidade "Absorver Alma de Cadáver" no corpo caído.
* **Critérios de Aceite**:
  * [ ] O cadáver se desintegra em um vórtice espiritual.
  * [ ] O colono recupera energia imediata e ganha XP proporcional ao tamanho do corpo.
  * [ ] Uma carta notifica que a linhagem foi catalogada no estágio **[Filhote / Newborn]**.
  * [ ] A criatura aparece imediatamente na galeria da aba "Alma".
  * [ ] Absorver um segundo cadáver da mesma criatura concede um salto de **+35%** de maturação.

---

### Teste 7: Metamorfose, Vácuo Espiritual e Restrição de Bancada
* **Objetivo**: Validar a troca de corpo, preservação de equipamentos e limites biológicos.
* **Passo a passo**:
  1. Equipe seu Portador com roupas (parka, calça, capacete) e uma arma de fogo ou espada.
  2. Abra a aba "Alma", localize a criatura catalogada e clique no botão **"Transformar (X E)"**.
  3. Verifique o visual do colono no mapa.
  4. Abra o painel de Equipamento/Inventário.
  5. Tente ordenar o colono metamorfoseado a pesquisar em uma bancada de pesquisa ou cozinhar no fogão.
  6. Recrute-o e ordene ataque corpo a corpo contra um inimigo.
  7. Clique no botão de gizmo ou na aba "Alma" em **"⟲ Reverter Forma Humana"**.
* **Critérios de Aceite**:
  * [ ] A energia é consumida apenas no instante da transformação (custo de remodelação biológica).
  * [ ] **Ficar transformado NÃO gasta energia contínua**: o colono pode permanecer na forma de fera indefinidamente e sua regeneração passiva de energia continua ativa.
  * [ ] O sprite desenhado no mapa muda perfeitamente para o animal selecionado.
  * [ ] Roupas e armas saem do corpo e são recolhidas ao vácuo espiritual sem cair no chão nem se perderem.
  * [ ] Trabalhos de alta precisão (fogão, pesquisa, alfaiataria) ficam desabilitados enquanto em forma de fera quadrúpede.
  * [ ] Movimentação, recrutamento e combate violento continuam sob controle direto do jogador.
  * [ ] Ao clicar em reverter, o corpo humano retorna e todas as roupas e armas originais reaparecem vestidas no colono.

---

### Teste 8: Maturação da Besta e Batismo das Leis
* **Objetivo**: Testar o avanço de estágio da fera e o avanço para a Classe Terra.
* **Passo a passo**:
  1. **Maturação da Besta**: Enquanto transformado na fera, ordene o colono a comer carne ou refeições. Verifique a barra de maturação aumentar. Ao atingir 100%, observe o surto de maturação avançando para **[Jovem / Juvenile]** e depois **[Adulto / Prime]**.
  2. **Batismo das Leis**: Medite ou use o ataque espiritual até a compressão atingir `200 / 200 XP`.
  3. Abra a aba "Alma" e clique no botão brilhante **"✦ Realizar Batismo das Leis ✦"**.
* **Critérios de Aceite**:
  * [ ] Mensagem de notificação ao amadurecer o estágio da linhagem da besta.
  * [ ] O botão do Batismo fica dourado e desbloqueado ao atingir 200 XP.
  * [ ] Relâmpagos azuis cobrem o colono e uma carta proclama o avanço para a **Classe Terra (Earth Class)**.
  * [ ] A capacidade máxima de Energia é expandida de 100 para 250.

---

### Teste 9: Avanço para Classe Céu & Feitiços Celestiais
* **Objetivo**: Validar o Batismo da Classe Céu, expansão para 500 de energia e novas habilidades.
* **Passo a passo**:
  1. Com o colono na Classe Terra, acumule 500 XP de compressão (através de meditação ou combate).
  2. Abra a aba Alma e clique em **✦ Realizar Batismo das Leis ✦**.
  3. Verifique o novo teto de Energia da Alma.
  4. Recrute o colono (*Draft*).
  5. Use a habilidade **Passo Celestial** clicando em um local atrás de muralhas ou a até 30 células de distância.
  6. Use a habilidade **Lança Celestial** contra um alvo hostil.
* **Critérios de Aceite**:
  * [ ] O título na aba Alma passa para **★ Classe Céu (Sky Class)**.
  * [ ] A Energia máxima salta para **500** e a regeneração sobe para **+75/hora**.
  * [ ] **Passo Celestial**: Transpõe instantaneamente o obstáculo em um salto celestial veloz com clarão cósmico ao aterrissar.
  * [ ] **Lança Celestial**: Causa 50 de dano direto (escalado por Condutividade e Crítico) e atordoa o alvo por 2 segundos.

---

### Teste 10: Atributos Primários Iniciais
* **Objetivo**: Validar a geração orgânica de atributos (STR, CON, AGI, SEN, CND) calibrada pelas habilidades e paixões vanilla do colono.
* **Passo a passo**:
  1. No jogo, clique na aba "Bio" de qualquer colono e observe os níveis e paixões em suas habilidades.
  2. Abra a aba **"Alma"**.
  3. Observe os valores calculados de **Força (STR)**, **Constituição (CON)**, **Agilidade (AGI)**, **Sentidos (SEN)** e **Condutividade (CND)**.
* **Critérios de Aceite**:
  * [ ] Habilidades sem paixão (pouco interesse) rendem **0.25** pontos por nível.
  * [ ] Habilidades com paixão normal (uma chama / com interesse) rendem **0.50** pontos por nível.
  * [ ] Habilidades com paixão ardente (duas chamas / muito interesse) rendem **1.20** pontos por nível.
  * [ ] Se a habilidade pertencer a mais de uma categoria, seus pontos foram divididos igualmente e arredondados para baixo.
  * [ ] A soma total de pontos iniciais respeita o mínimo de **25** e o máximo de **110**.

---

### Teste 11: Progressão de Níveis, XP por Tarefas/Feitiços & Ferramentas DEV
* **Objetivo**: Testar ganho de XP por tarefas da colônia, proporção de XP por feitiços (50 MP = 25 XP), ferramentas DEV (+1 e +10), e bônus de ascensão com XP acumulado.
* **Passo a passo**:
  1. Abra a aba "Alma" e verifique a indicação de nível: `★ Classe Mortal (Mortal Class) - Nível 1 / 10`.
  2. **XP por Tarefas**: Ordene o colono a minerar, construir ou fabricar. Observe a barra de XP subir.
     * Verifique que no nível 0 da habilidade ele ganha 1 XP por ponto de trabalho; em níveis superiores o ganho multiplica por `(1 + nível)`.
     * Verifique a paixão na habilidade: baixa = metade do XP (0.5x), normal = 1x, ardente = dobro (2x).
  3. **XP por Feitiços**: Lance o Disparo da Alma (custa 20 MP). Observe que ganha exatamente **10 XP** (50% do MP gasto).
  4. **Ferramenta DEV**: Ative o Dev Mode do jogo. Na aba Alma, clique no botão **[+1 Nv]** ou **[+10]** (ou use os comandos DEV do colono).
     * Observe o salto imediato de nível e a concessão de +5 (ou +50) pontos livres para distribuir.
  5. **Gargalo e Batismo das Leis**: Ao atingir o Nível 10 com XP cheio, clique em **✦ Realizar Batismo das Leis ✦**.
  6. **Distribuição Rápida de Atributos**: Observe que cada linha de atributo agora possui dois botões: **[+]** para adicionar 1 ponto e **[+5]** para adicionar 5 pontos diretamente com um único clique.
* **Critérios de Aceite**:
  * [ ] O trabalho na colônia e o combate mágico progridem o cultivo da alma de forma consistente.
  * [ ] Botões DEV permitem saltar 1 ou 10 níveis instantaneamente para testes.
  * [ ] O botão **[+5]** adiciona 5 pontos instantaneamente ao atributo escolhido, agilizando a distribuição.
  * [ ] Ao realizar o Batismo para a Classe Terra, o colono ganha **+50 pontos de atributos** (equivalente a todo o avanço do estágio anterior)!
  * [ ] O Nível 1 da Classe Terra exige exatamente **1.593 XP** (a soma acumulada de todos os 10 níveis da Classe Mortal).

---

### Teste 12: Nova Escala de Poder dos Atributos, Amplitude Térmica & Evasão Balística
* **Objetivo**: Validar a escala de poder recalculada, incluindo tolerância a frio/calor extremos, esquiva de projéteis balísticos, golpes críticos letais e velocidade contínua sem teto.
* **Passo a passo**:
  1. Na aba "Alma", observe os 5 Atributos e suas novas linhas informativas e tooltips aprofundados.
  2. **Constituição & Amplitude Térmica**: Aloque 50 ou 100 pontos em **Constituição (CON)**.
     * Abra a aba "Informações" do colono e verifique a faixa de temperatura confortável: ela deve expandir em $\pm 25^\circ\text{C}$ (50 CON) ou $\pm 50^\circ\text{C}$ (100 CON). O colono não sofre hipotermia em biomas congelados nem insolação em desertos escaldantes.
     * Verifique que a taxa de fome (`HungerRateMultiplier`) cai substancialmente e ferimentos cicatrizam em velocidade quádrupla.
  3. **Condutividade & MP Exponencial**: Aloque 50 ou 100 pontos em **Condutividade (CND)**.
     * Verifique o teto de MP: com 50 CND atinge 250 MP (+150%); com 100 CND atinge 400 MP (+300%).
  4. **Agilidade & Esquiva de Tiros**: Aloque pontos em **Agilidade (AGI)**.
     * Recrute o colono e observe que a velocidade de movimento (`MoveSpeed`) cresce linearmente sem teto (+0.06 c/s por ponto).
     * Peça para um inimigo ou aliado atirar contra o cultivador. Observe o colono desviar de balas em pleno ar com partículas de poeira e o texto flutuante **"ESQUIVOU!"**.
  5. **Sentidos & Críticos Letais / Força & Atordoamento**:
     * Ordene o colono a atacar inimigos com armas ou garras em combate corporal.
     * Observe o surgimento de **"CRÍTICO! (2.0x)"** em amarelo com impacto massivo (SEN) e **"ATORDOADO!"** (STR).
* **Critérios de Aceite**:
  * [ ] CON expande a tolerância térmica a frio e calor em $\pm 0.5^\circ\text{C}$ por ponto e reduz fome/sono.
  * [ ] CND amplia o teto de energia espiritual em $+3\%$ por ponto multiplicativo sobre a base.
  * [ ] AGI concede velocidade contínua sem teto e esquiva contra tiros de armas de fogo com feedback visual `"ESQUIVOU!"`.
  * [ ] SEN e STR disparam críticos multiplicados ($1.5\text{x}$ até $3.0\text{x}$) e atordoamentos físicos em combate.

---

### Teste 13: Sub-Aba Grimório de Técnicas, Maestria e Foco na Meditação
* **Objetivo**: Validar a navegação na sub-aba do Grimório, visualização de níveis de maestria e ganho de proficiência passiva na meditação.
* **Passo a passo**:
  1. Abra a aba **"Alma"** de qualquer colono cultivador.
  2. Clique no botão superior **[📖 Grimório de Técnicas 📖]**.
  3. Observe a lista de habilidades desbloqueadas correspondentes ao estágio do colono.
  4. Localize a técnica **"Flecha de Fogo"** (ou qualquer outra).
  5. Clique no botão **[Focar (+10/h)]**. Observe o banner superior atualizar para `★ Foco Atual na Meditação: Flecha de Fogo`.
  6. No Dev Mode, clique no botão **[+25]** na linha da habilidade para avançar a proficiência.
  7. Ao atingir 100%, observe o salto de grau de maestria (ex: de `[Iniciado]` para `[Adepto]`).
  8. Ordene o colono a **Meditar na Matriz** e acelere o tempo.
* **Critérios de Aceite**:
  * [ ] A sub-aba alterna suavemente entre *Visão Geral & Atributos* e *Grimório de Técnicas*.
  * [ ] Cada habilidade exibe seu Grau (`Iniciado`, `Adepto`, `Perito`, `Mestre`), multiplicadores de custo e poder, e barra de proficiência.
  * [ ] O botão `★ Foco Ativo` permite fixar ou desmarcar a técnica de foco.
  * [ ] Durante a meditação contínua, a técnica com foco ativo ganha **+10 de proficiência por hora** de cultivo.
  * [ ] Ao atingir o grau Mestre, a barra exibe `★ MESTRIA MÁXIMA ★` e o custo é reduzido em 25% com +50% de potência.

---

### Teste 14: As 10 Novas Técnicas de Cultivo em Ação
* **Objetivo**: Validar a execução mecânica das 10 novas habilidades.
* **Passo a passo**:
  1. **Passo do Vento (Wind Dash)**: Custa 25 SP. Clique no chão a até 8 células; o colono desliza em velocidade supersônica deixando rastro de vento sem gastar MP.
  2. **Flecha de Fogo (Fire Arrow)**: Custa 15 MP. Dispare contra um inimigo ou ponto do chão; causa 20 de dano de fogo e ateia chamas no alvo. Concede +7.5 XP.
  3. **Curar (Heal)**: Custa 25 MP (60 MP no grau Mestre). Lance sobre si mesmo ou aliado ferido; restaura ferimentos instantaneamente. No grau Mestre, restaura membros perdidos ou remove cicatrizes.
  4. **Pressa (Haste)**: Custa 20 MP e 15 SP. Ative para receber o hediff de Pressa por 30s (+50% velocidade de movimento e +40% velocidade de trabalho).
  5. **Pele de Pedra (Stone Skin)**: Custa 35 MP (Classe Terra). Ative para receber +60% Sharp, +50% Blunt e +40% Heat com -20% de mobilidade por 30s.
  6. **Regeneração Acelerada (Accelerated Regen)**: Custa 40 MP (Classe Terra). Ative para receber +400% cura natural e cicatrização automática de 1.5 HP/s no ferimento mais grave por 1 dia in-game (24 horas).
  7. **Raio de Gelo (Frost Ray)**: Custa 30 MP (Classe Terra). Dispare contra um inimigo; causa 30 de dano e aplica hipotermia grave (+40%).
  8. **Arco Voltaico (Chain Lightning)**: Custa 50 MP (Classe Céu). Dispare contra um grupo de inimigos; causa 45 de dano elétrico, atordoa por 2.5s e salta em cadeia por até 5 alvos.
  9. **Teleporte (Teleport)**: Custa 40 MP (Classe Céu). Teleporta o usuário para qualquer local visível no alcance (20 células no grau 1 até mapa inteiro no grau Mestre).
  10. **Benção dos Céus (Blessing of the Heavens)**: Custa 80 MP e 40 SP (Classe Céu). Concede o Escudo Psiônico divino de 750 HP e a bênção de 45s que reúne blindagem quíntupla (sem perda de mobilidade) e regeneração multiplicada por 10.
* **Critérios de Aceite**:
  * [ ] Todas as 10 habilidades funcionam com seus custos exatos de MP/SP e concedem 50% do custo em Soul XP.
  * [ ] As restrições de classe (Classe Terra e Céu) impedem conjurações antes da ascensão necessária.

---

### Teste 15: Escudo Psiônico — Absorção Unilateral & Disparo de Dentro
* **Objetivo**: Validar a absorção de dano do Escudo Psiônico e confirmar que o cultivador pode atacar e disparar normalmente de dentro do escudo.
* **Passo a passo**:
  1. Recrute o cultivador e ative a habilidade **"Escudo Psiônico"** (ou use a *Benção dos Céus*).
  2. Observe o hediff `Escudo Psiônico [150/150 HP]` na lista de condições de saúde.
  3. Equipe uma arma de disparo no colono (ou use o feitiço *Disparo da Alma*).
  4. **Disparo de Dentro**: Ordene que ele atire em um alvo inimigo distante.
     * Verifique que o escudo **NÃO impede** o disparo (diferente dos shield belts vanilla).
  5. **Absorção de Dano**: Provoque um ataque contra o colono com o escudo ativo.
     * Observe o texto flutuante `-XX ESCUDO` e as partículas azuis psíquicas.
     * O HP do escudo diminui sem causar nenhum dano corporal ao colono.
  6. Quando o total de dano sofrido atingir 150 HP, o escudo colapsa com efeito sonoro e texto flutuante *"ESCUDO COLAPSADO!"*.
* **Critérios de Aceite**:
  * [ ] O escudo absorve qualquer tipo de ataque externo (físico ou mágico) até o esgotamento do HP.
  * [ ] O colono pode conjurar feitiços e atirar livremente de dentro da proteção.
  * [ ] Nenhum erro é disparado no log durante o combate com escudo ativo.

---

### Teste 16: Projéteis Balísticos Voadores & Bloqueio Total de Efeitos por Escudo
* **Objetivo**: Validar que as habilidades de ataque à distância disparam projéteis físicos voadores nativos e que escudos (tanto o *Escudo Psiônico* quanto cintos de escudo vanilla) bloqueiam o dano e impedem completamente a aplicação de efeitos secundários nocivos (fogo, hipotermia, atordoamento).
* **Passo a passo**:
  1. **Disparo em Voo (Física e Balística)**:
     - Com um cultivador recrutado a 15-20 células de distância de um alvo, dispare:
       - *Disparo da Alma* (`REM_Proj_SoulBolt`): projétil de energia azul celeste viajando a vel 65.
       - *Flecha de Fogo* (`REM_Proj_FireArrow`): projétil incandescente viajando a vel 55.
       - *Raio de Gelo* (`REM_Proj_FrostRay`): dardo congelante ciano viajando a vel 70.
       - *Lança Celestial* (`REM_Proj_SkyLance`): lança dourada de alta velocidade viajando a vel 85.
       - *Arco Voltaico* (`REM_Proj_ChainLightning`): descarga elétrica viajando a vel 75.
     - Observe que os projéteis levam frações de segundo para cruzar o ar até o alvo.
  2. **Alvo Sem Escudo (Efeitos Secundários Ativos)**:
     - Atinja um alvo desprotegido com a *Flecha de Fogo*: observe que o alvo sofre dano de fogo e pega fogo imediatamente.
     - Atinja um alvo desprotegido com o *Raio de Gelo*: observe que o alvo sofre hipotermia severa (+40%).
     - Atinja um alvo desprotegido com o *Arco Voltaico*: observe que o alvo inicial é atordoado por 2.5s e a descarga salta para inimigos adjacentes com faíscas.
  3. **Alvo Com Escudo Psiônico ou Cinto de Escudo (Efeitos Secundários Bloqueados)**:
     - Ative o *Escudo Psiônico* em um colono (ou coloque um cinto de escudo vanilla em um prisioneiro/alvo).
     - Dispare a *Flecha de Fogo* contra o alvo com escudo ativo:
       - O dano é absorvido pelo escudo (reduzindo o HP do escudo com som característico e texto `-XX ESCUDO`).
       - **O alvo NÃO pega fogo!** Nenhum fogo ou combustão é iniciado enquanto o escudo absorver o impacto.
     - Dispare o *Raio de Gelo* contra o alvo com escudo ativo:
       - O dano é absorvido pelo escudo.
       - **O alvo NÃO recebe hipotermia!**
     - Dispare o *Arco Voltaico* contra o alvo com escudo ativo:
       - O dano inicial é absorvido pelo escudo.
       - **O alvo NÃO é atordoado!** (E qualquer alvo em cadeia subsequente protegido por escudo também absorve sem ser atordoado).
  4. **Esgotamento do Escudo**:
     - Dispare sucessivas vezes até o escudo do alvo colapsar (`ESCUDO COLAPSADO!`).
     - No disparo seguinte que atingir a carne do alvo, os efeitos secundários voltam a ser aplicados normalmente.
* **Critérios de Aceite**:
  * [ ] Todos os 5 feitiços à distância viajam como projéteis físicos balísticos pelo ar sem depender de JecsTools.
  * [ ] Se o impacto for absorvido por escudo, **NENHUM efeito adicional** (fogo, hipotermia, atordoamento) vaza para o alvo.
  * [ ] Se o alvo não tiver escudo ou o escudo tiver quebrado, todos os efeitos nocivos secundários funcionam 100%.

---

### Teste 17: Animações, VFX Multi-Camada, Raios em Malha, Flyers e Cúpula de Escudo
* **Objetivo**: Validar o novo sistema completo de animações visuais/sonoras nativo inspirado em *RoM* e *Kure's*, verificando todos os sprites customizados, rastros de projéteis, arcos elétricos reais, voos de dash, cúpula de escudo hexagonal, portais, auras e pilares de ascensão sem erros no log.
* **Passo a passo**:
  1. **Sprites & Rastros de Projéteis**:
     - Dispare os 5 projéteis (*Disparo da Alma*, *Flecha de Fogo*, *Raio de Gelo*, *Lança Celestial*, *Arco Voltaico*).
     - Verifique no conjurador a aparição de círculos rúnicos duplos contra-rotativos sob os pés no momento da conjuração.
     - Observe os novos sprites luminosos de cada magia e seus rastros em voo (chamas em *Flecha de Fogo*, cristais de gelo em *Raio de Gelo*, filetes dourados em *Lança Celestial*).
     - Observe no impacto: onda de choque expansiva (`ShockwaveRing`), vórtice, partículas elementais irradiando e som correspondente.
  2. **Arcos Elétricos Reais (`RebornLightningArc`)**:
     - Dispare *Arco Voltaico* em um grupo de 2+ inimigos próximos.
     - Verifique o feixe elétrico irregular cintilante de malha dupla (núcleo branco + aura azul-elétrica) ligando conjurador -> alvo primário -> inimigos secundários em cadeia.
  3. **Movimento Fluido (`WindDash`, `SkyStep`) e Portais (`Teleport`)**:
     - Use *Passo do Vento*: o colono desliza rapidamente pelo terreno (`REM_Flyer_WindDash`) deixando pós-imagens e filetes esverdeados até aterrissar com onda de choque; a seleção e o estado *Drafted* são mantidos ao final.
     - Use *Passo Celestial*: o colono salta em arco pelos ares transpondo obstáculos/paredes (`REM_Flyer_SkyStep`) deixando faíscas celestiais e aterrissando com arranjo rúnico dourado-ciano.
     - Use *Teleporte*: observe a abertura do portal espiral de partida (`SwirlIn` + runa) e a eclosão expansiva do portal de destino (`ShockwaveRing` + `SwirlOut` + som Skip).
  4. **Cúpula do Escudo Psiônico & Auras Contínuas**:
     - Ative *Escudo Psiônico*: observe uma cúpula hexagonal translúcida giratória envolvendo o colono.
     - Ataque o colono protegido: veja o escudo piscar em branco/ciano e emitir onda de choque na absorção, mudando de cor rumo ao âmbar se o HP cair.
     - Ative *Pele de Pedra*, *Pressa*, *Regeneração Acelerada* e *Bênção dos Céus*: observe cristais terrosos, faíscas velozes, pétalas verdes ascendentes e o círculo rúnico dourado sob os pés com orbes orbitais.
  5. **Rituais e Pilares de Ascensão**:
     - Realize *Meditar na Matriz* (runa calma + faíscas espirituais), *Absorver Alma de Cadáver* (feixe de drenagem espiritual conectando o cadáver ao conjurador), e ative *Batismo das Leis* / *Cristal de Despertar* / *Pérola Mágica* (pilar vertical de luz dos céus + dupla onda de choque + som de trovão).
* **Critérios de Aceite**:
  * [ ] Zero erros vermelhos no `Debug Log` relacionados a texturas ausentes, shaders, `FleckDef`, `PawnFlyer` ou `RebornLightningArc`.
  * [ ] Todos os projéteis, portais, voos (`WindDash`/`SkyStep`), arcos elétricos, cúpula de escudo e auras são renderizados suavemente em jogo.

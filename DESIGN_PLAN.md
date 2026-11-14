# Documento de Design e Arquitetura Oficial
# Mod: Reborn: Evolving From Nothing — Magic & Soul Matrix

---

## 1. Visão Geral & Lore Central (Fiel aos 30.000 Mundos)

O mod traduz para o RimWorld o sistema de poder e a premissa central da obra *Reborn: Evolving From Nothing* (AuthorWiz).

### 1.1 A Realidade dos 30.000 Mundos:
* **A Matriz de Feitiços da Alma (*Soul Spell Matrix*) é Universal**:
  * Toda criatura viva (seja um colono humano comum, um muffalo, um megasscaravelho ou um dragão antigo) possui nativamente uma Soul Spell Matrix.
  * A Soul Matrix mede a densidade da alma, o Estágio da Alma (*Soul Stage*) e o teto máximo de energia suportado pelo corpo.
  * Seres comuns estão **presos rigidamente à sua própria espécie pelo resto da vida**.
  * **O Teto Racial e o Caminho do Cultivo Humano**:
    * Um humano comum não refinado possui teto natural na **Classe Mortal**.
    * Para que o corpo humano suporte a densidade da **Classe Terra** e além, ele precisa passar pelo **Caminho do Cultivo Humano** (meditação disciplinada, compressão da alma e o Batismo das Leis da Magia).
    * Sem cultivo, o corpo biológico comum colapsaria se tentasse reter a energia de um estágio superior.

* **A Anomalia do Protagonista — A Pérola Mágica (*The Magic Pearl*)**:
  * Um único indivíduo na colônia (o Reencarnado) possui sua Soul Matrix vinculada ao poder de um artefato lendário: **A Pérola Mágica**.
  * Esse vínculo quebra as leis naturais do universo, conferindo 4 vantagens exclusivas:
    1. **Absorção e Catalogação de Linhagens**: Ao devorar ou canalizar cadáveres frescos de feras ou monstros, a Pérola extrai, analisa e salva o código genético/espiritual da criatura em um catálogo vivo sem colapso mental.
    2. **Metamorfose e Troca Livre de Corpos**: O indivíduo pode remodelar seu recipiente físico para assumir a forma visual, mecânica e combativa de qualquer criatura catalogada (corpos blindados, rápidos, pesados, voadores, etc.).
    3. **Superação do Teto Racial (Quebra de Gargalos)**: Quando uma forma atinge seu limite de energia suportado pela carne, o portador absorve e evolui para uma espécie superior para continuar avançando de classe.
    4. **Fusão e Domínio de Feitiços da Alma (*Soul Spells*)**: Capacidade de aprender e manifestar habilidades inatas das bestas assimiladas.

---

## 2. Métodos de Início & Despertar da Pérola Mágica

O jogador tem total flexibilidade narrativa para introduzir o Reencarnado:

1. **Início Narrativo / Cenário (Trait de Nascimento)**:
   * **Trait**: `REM_Trait_MagicPearlBearer` (*Portador da Pérola Mágica* / *O Primeiro Reencarnado*).
   * O colono selecionado na criação do jogo já inicia com a Pérola fundida à sua alma e sua primeira linhagem base registrada.
2. **Descoberta em Jogo (O Artefato Único)**:
   * **Item**: `REM_Item_MagicPearl` (*A Pérola Mágica*).
   * Um artefato misterioso de origem celestial que pode ser encontrado através de missões ancestrais raras, ruínas seladas ou comerciantes estelares.
   * Ao ser usado por um colono, a Pérola se funde permanentemente à sua alma (consumindo o item físico), concedendo o Trait e ativando o painel de linhagens.

---

## 3. Os Dois Caminhos de Progressão

### 3.1 Colonos Comuns (Cultivo Humano Padrão)
* Todos os colonos possuem a **Aba "Alma"** na janela de inspeção.
* Eles acumulam Energia da Alma e XP de compressão através de:
  * Meditação em transe (*Soul Meditation*).
  * Estudo de símbolos arcanos e leitura de manuscritos das Leis.
  * Prática em combate.
* Seus corpos continuam 100% humanos; eles aprendem feitiços elementais canônicos conforme avançam nas classes (Mortal $\rightarrow$ Earth $\rightarrow$ Sky...), mas não podem se transformar ou absorver corpos de monstros.

### 3.2 O Portador da Pérola Mágica (O Caminho da Metamorfose e Evolução)
* Possui todas as capacidades de cultivo de um colono comum.
* Possui o feitiço exclusivo **Absorver Matriz da Alma (*Soul Extraction*)**:
  * Ao canalizar cadáveres recentes no mapa, consome a carne e a alma da criatura.
  * **Compatibilidade Total**: Qualquer criatura/animal do RimWorld (vanilla ou de mods instalados, como Alpha Animals) pode ser catalogada no banco genético da Pérola!
  * Recupera energia da alma e adiciona a espécie ao catálogo de transformações.

* **Mecânica Rigorosa de Metamorfose (Fiel à Obra)**:
  1. **Tempo de Canalização / Remodelação Biológica**:
     * Nos estágios iniciais (*Mortal*, *Earth*, *Sky*), a metamorfose **não é instantânea**. O colono precisa de uma canalização lenta (vários segundos parado enquanto ossos e tecidos se reconstroem), tornando perigoso mudar de forma exposto em combate aberto.
     * Nos estágios avançados (*Lord*, *King*, *Angelic*), o domínio corporal atinge o ápice, reduzindo a transição para frações de segundo e permitindo mutações parciais.
  2. **4 Estágios de Maturação da Linhagem**:
     * Cada espécie catalogada entra no banco genético no estágio **Newborn (Filhote)**.
     * O portador precisa nutrir essa linhagem com energia e carne consumida para amadurecê-la através dos estágios:
       * `Newborn` (Filhote) $\rightarrow$ `Juvenile` (Jovem) $\rightarrow$ `Prime` (Adulto / Pico) $\rightarrow$ `Elder` (Ancião).
     * **Mecanismos de Maturação no Jogo**:
       * *Consumo e Alimentação na Forma*: Enquanto transformado naquela forma, alimentar-se de carne, corpos ou recursos nutritivos canaliza biomassa diretamente para o medidor de maturação da fera.
       * *Assimilação de Mesma Espécie*: Absorver cadáveres adicionais da mesma criatura (ou espécimes adultos caídos) concede um salto maciço de completude genética.
       * *Surto de Maturação (Reclusão)*: Ao atingir 100% de maturação do estágio atual, o portador passa por um breve surto de remodelação física (muda de pele/casca e expansão de porte físico), consolidando o próximo estágio.
     * Somente no estágio *Prime* ou superior a fera atinge seu tamanho corporal completo (`BodySize`), força máxima e todas as habilidades biológicas inatas.
  3. **Custo Energético & Estabilidade Corporal (`BodySize`)**:
     * A transição consome uma quantidade de Energia da Alma proporcional à diferença de massa entre o corpo de partida e o corpo de destino **apenas no momento da ativação** (o custo biológico de remodelar ossos e tecidos).
     * Mudar para uma besta colossal (como um Megatério, Rinoceronte ou Dragão) consome uma fatia considerável de energia para criar a massa física biológica inicial.
     * **Sem Dreno Contínuo**: Uma vez concluída a remodelação celular, a forma é anatomicamente estável e **permanecer transformado NÃO custa energia contínua**. O colono pode viver e lutar nessa forma pelo tempo que desejar, e sua regeneração passiva de energia continua funcionando normalmente.
  4. **Vácuo Espiritual de Equipamento & Controle Comportamental**:
     * Todas as roupas, armaduras e armas humanas empunhadas são guardadas em uma dimensão de bolso espiritual durante a transformação e reaparecem vestidas/empunhadas perfeitamente quando o colono desativa a metamorfose e retorna à forma humana.
     * **Comportamento em Forma de Besta**: O portador mantém 100% da consciência humana e o controle total do jogador (pode ser recrutado/drafted, mover-se e atacar alvos específicos sob comando), porém **perde a capacidade de manipular ferramentas humanas complexas** (não pode cozinhar em fogões, pesquisar ou operar bancadas de alfaiataria enquanto estiver na forma de animal quadrúpede sem polegares opositores). A qualquer momento fora de combate, o jogador pode desativar a transformação para reassumir tarefas civis.

---

## 4. Design da Interface do Usuário (UI/UX)

### 4.1 Aba "Alma" (Matriz Tab)
Presente em todos os colonos, mas adaptativa:
1. **Para Colonos Comuns**:
   * Painel de Energia da Matriz (atual / máxima e taxa de regeneração).
   * Barra de Compressão (XP da Classe).
   * Botão de Batismo das Leis (*Breakthrough*).
2. **Para o Portador da Pérola Mágica**:
   * Inclui uma seção adicional: **"Galeria de Linhagens Catalogadas"**.
   * Exibe miniaturas/cards das criaturas assimiladas (ex: *Salamandra Vermelha*, *Lobo do Gelo*, *Megasscaravelho*, etc.).
   * Clicar em uma fera ativa a **Metamorfose Instantânea** (com tempo de canalização e custo de energia proporcional ao nível da fera).
   * Botão "Reverter para Forma Humana".

### 4.2 Indicador em Combate (Overhead Bar)
* Quando o colono (ou sua forma metamórfica) for recrutado (*Drafted*), uma barra sutil de Energia da Alma é renderizada sobre a cabeça, permitindo acompanhar o consumo de energia sem poluir a tela com menus.

---

## 5. Arquitetura do Sistema de Status do Cultivador Humano

### 5.1 Os 5 Atributos Primários (Base do Cultivo)
1. **Força (STR)**:
   * Escala o dano de ataque físico corpo a corpo (+0.8% de `MeleeDamageFactor` por ponto).
   * Eleva a capacidade de transporte (+0.5 kg de carga por ponto).
   * Contribui para a Estamina Máxima (+1 SP por ponto).
   * Força de empurrão e penetração de armadura natural física (curva assintótica até 60%).
2. **Constituição (CON)**:
   * Eleva os Pontos de Vida / Integridade Física (+0.5% de `HealthScale` por ponto).
   * Mitigação passiva de dano recebido através de curva assintótica: `Fator = 1.0 - (0.85 * CON / (CON + 150))`.
   * Eleva a Estamina Máxima (+1.5 SP por ponto) e acelera a taxa de regeneração de SP (+1% por ponto).
   * Acelera a cicatrização e regeneração de ferimentos (+0.5% por ponto).
3. **Agilidade (AGI)**:
   * Velocidade de deslocamento e passos no grid sem teto máximo (+0.05 células/s linear contínuo por ponto).
   * Chance de esquiva em combate corpo a corpo (curva assintótica com teto de 65%: `0.65 * (AGI / (AGI + 250))`).
   * Cadência e velocidade de execução de golpes físicos (redução de cooldown melee até 50%).
4. **Sentidos (SEN)**:
   * Alcance de visão (+0.2% de `Sight` por ponto) e precisão de disparo (+0.2% de `ShootingAccuracy` por ponto).
   * Chance de acerto crítico físico e mágico (curva assintótica até 50%: `0.50 * (SEN / (SEN + 300))`).
   * Aceleração da regeneração passiva de Energia da Alma (+0.2% por ponto).
5. **Condutividade (CND)**:
   * Expansão da reserva máxima de Energia da Alma / Éter (+2 MP por ponto).
   * Potência e amplificação de dano de todos os feitiços da alma (+0.8% por ponto).
   * Resistência a dano espiritual e psíquico.

### 5.2 Recursos Vitais e Sobrevivência
* **Pontos de Vida (HP)**: Integridade dos tecidos biológicos, escalada por Constituição.
* **Estamina / Vigor (SP)**: Barra verde dedicada para esforço físico e técnicas marciais.
  * Teto Máximo: `100 + (CON * 1.5) + (STR * 1.0)`.
  * Esgotamento (0 SP): Apenas bloqueia a execução de técnicas marciais especiais (o colono continua andando e atacando normalmente sem desmaios punitivos).
  * Recuperação: Regeneração rápida fora de esforço contínuo.
* **Energia da Alma / Éter (MP)**: Barra azul da Matriz gasta em feitiços da alma e mobilidade celeste.
* **Saciedade & Biomassa**: Nutrição do organismo.

### 5.3 Geração Dinâmica de Atributos Iniciais por Habilidades Vanilla
Ao despertar a alma, os pontos iniciais de cada colono são calculados organicamente por suas vocações e paixões vanilla:
* Cada nível de habilidade inicial (0 a 20) concede:
  * **0.25 pontos** para habilidades com pouco interesse (`Passion.None`).
  * **0.50 pontos** para habilidades com interesse (`Passion.Minor`).
  * **1.20 pontos** para habilidades com muito interesse (`Passion.Major`).
* Se uma habilidade pertencer a mais de uma categoria, seus pontos são divididos igualmente; frações arredondam para baixo:
  * **Força**: Tiro, Corpo a corpo, Animais.
  * **Constituição**: Corpo a corpo, Mineração, Fabricação.
  * **Agilidade**: Medicina, Fabricação, Culinária.
  * **Sentidos**: Intelecto, Social, Arte.
  * **Condutividade**: Arte, Intelecto, Plantas.
* Limites de segurança inicial: **Mínimo de 25 pontos totais** e **Máximo de 110 pontos totais**.

### 5.4 Progressão de Níveis por Estágio (Hierarquia dos 9 Tiers)
* **Fórmula de Níveis por Estágio**: Cada estágio possui exatamente `Número do Estágio * 10` níveis (Mortal: 10, Terra: 20, Céu: 30...).
* **Ganho por Nível**: A cada nível alcançado, o personagem recebe **5 Pontos Livres de Atributo**.
* **Curva Exponencial de XP**: Cada nível exige **10% a mais de XP** que o nível anterior: `XP = BaseXP * (1.10 ^ (NivelAtual - 1))`.
* **Escala Inter-Estágios (Regra Suprema)**:
  * O XP requerido para a primeira fase (Nível 1) do próximo estágio é a quantidade de XP necessária durante **todo o estágio anterior acumulado**.
  * Exemplo: O Nível 1 da Classe Terra exige `1.593 XP` (a soma dos 10 níveis da Classe Mortal). O Nível 1 da Classe Céu exige `~91.239 XP` (a soma dos 20 níveis da Classe Terra).
* **Fontes de XP**:
  * **Tarefas Básicas e Trabalho na Colônia**: Concedem 1 XP por ponto de trabalho no nível 0 da habilidade correspondente. Para cada nível superior, o ganho aumenta em 1 (`1 + nível`). Modificado por paixão:
    * Baixa / Sem paixão: metade do XP (`0.5x`).
    * Normal / Com interesse: ganho padrão (`1.0x`).
    * Ardente / Muito interesse: dobro do XP (`2.0x`).
  * **Habilidades e Feitiços**: Concedem experiência equivalente a **50% da energia espiritual gasta** (proporção: 50 MP rendem 25 XP). Ex: Disparo da Alma (20 MP = 10 XP), Passo Celestial (30 MP = 15 XP), Lança Celestial (35 MP = 17.5 XP).
  * **Meditação na Matriz**: Tarefa dedicada com circulação contínua (+100 XP/h e +100 MP/h).
* **O Batismo das Leis (Breakthrough)**:
  * Ao completar o nível máximo do estágio com a barra de XP cheia, o Batismo das Leis é liberado.
  * **Bônus de Ascensão**: Saltar de estágio garante **pontos de atributos equivalente a todos os pontos ganhos durante todo o estágio anterior**!
    * Ao romper Mortal $\rightarrow$ Terra: Ganha **+50 pontos de atributos** (10 níveis x 5 pts)!
    * Ao romper Terra $\rightarrow$ Céu: Ganha **+100 pontos de atributos** (20 níveis x 5 pts)!
    * Ao romper Céu $\rightarrow$ Mestre: Ganha **+150 pontos de atributos** (30 níveis x 5 pts)!
* **Ferramentas de Desenvolvedor (Dev Tools)**:
  * Botões `[DEV +1 Nv]` e `[DEV +10 Nv]` disponíveis no modo de desenvolvedor para testes ágeis na Aba "Alma" e nos gizmos do peão.

### 5.5 Especificação dos Estágios Atuais (Mortal, Terra e Céu)
* **Classe Mortal (Tier 1)**: 10 níveis, base 100 XP, teto inicial de 100 MP, regeneração base +15/h. Feitiços: Meditar na Matriz e Disparo da Alma.
* **Classe Terra (Tier 2)**: 20 níveis, base 250 XP, teto inicial de 250 MP, regeneração base +40/h.
* **Classe Céu (Tier 3)**: 30 níveis, base 500 XP, teto inicial de 500 MP, regeneração base +75/h. Feitiços: Passo Celestial (transposição de até 30 células) e Lança Celestial (50 dano direto + atordoamento).

### 5.6 Mecânica Futura Documentada: Sincronia (Corpo × Alma %)
* A taxa de equilíbrio entre o nível da alma e o recipiente físico será implementada em fase futura, modulando bônus de harmonia (até +30% quando acima de 90%) ou penalidades se o corpo não for treinado para conter a pressão da alma.
---


## 6. Registro de Implementação (Fase 1 Concluída)

Todas as fundações foram implementadas, compiladas para RimWorld 1.6 e sincronizadas:

### 6.1 Classes C# Criadas e Atualizadas
* `Source/Core/CatalogedBloodline.cs`:
  * Gerenciamento de espécies catalogadas, 4 estágios de maturação (`Newborn` $\rightarrow$ `Juvenile` $\rightarrow$ `Prime` $\rightarrow$ `Elder`), multiplicadores dinâmicos de escala física, dano, velocidade e vida, e cache de renderização gráfica.
* `Source/Core/Hediff_MetamorphicForm.cs`:
  * Hediff dinâmico de transformação física, garantindo bônus combativos da fera, bloqueio de tarefas industriais complexas e comando "Reverter Forma Humana".
* `Source/Core/Hediff_SoulMatrix.cs`:
  * Gerenciamento da Pérola Mágica, bolso dimensional para roupas e armas (`spiritualApparel`, `spiritualWeapon`), cálculo de custo de energia baseado na massa (`BodySize`), dreno de manutenção e evolução de linhagem através da ingestão de comida e absorção de cadáveres.
* `Source/Items/CompUseEffect_BindMagicPearl.cs`:
  * Item utilizável para fundir a Pérola Mágica à alma do colono selecionado.
* `Source/Abilities/MortalAbilities.cs`:
  * Atualização da habilidade `REM_Ability_AbsorbSoul` para alimentar o catálogo da Pérola Mágica com saltos de maturação de +35% ao consumir corpos da mesma espécie e +10% para a forma ativa.
* `Source/Harmony/RebornHarmonyPatches.cs`:
  * Injeção da Aba "Alma" no Inspector.
  * Renderização em tempo real do sprite animal (`DynamicDrawPhaseAt` e `RenderPawnAt`).
  * Restrição de tarefas de alta precisão em forma de fera (`WorkTypeIsDisabled`).
  * Barra de combate overhead sutil durante drafting.
* `Source/UI/ITab_Pawn_SoulMatrix.cs`:
  * Interface adaptativa: para o Portador da Pérola Mágica, expande com a **Galeria de Linhagens Catalogadas**, status da forma ativa, barras de maturação individual e botão de transformação em 1 clique.

### 6.2 Defs Registrados
* `Defs/TraitDefs/Traits_Reborn.xml`:
  * `REM_Trait_MagicPearlBearer`: Trait inicial do Reencarnado.
* `Defs/ThingDefs_Items/Items_SoulMatrix.xml`:
  * `REM_Item_MagicPearl`: Artefato utilizável para fundir a Pérola à alma em jogos existentes.
  * `Make_REM_MagicPearl`: Receita de condensação mística.
* `Defs/HediffDefs/Hediffs_SoulMatrix.xml`:
  * `REM_Hediff_MetamorphicForm`: Hediff de transformação ativa.
* `Defs/AbilityDefs/Abilities_Mortal.xml`:
  * `REM_Ability_Meditate`: Feitiço base universal de meditação (+100 Energia/h, +100 XP/h).
  * `REM_Ability_SoulBolt`: Feitiço base universal de ataque espiritual (Disparo da Alma, 25 dano direto, 20 custo, +8 XP).
  * `REM_Ability_AbsorbSoul`: Feitiço exclusivo do Portador da Pérola Mágica para extrair almas e códigos genéticos de cadáveres.

---

## 7. Roteiro e Procedimentos de Teste em Jogo

O checklist completo e passo a passo para validação manual de cada mecânica está documentado em:
* [TEST_GUIDE.md](file:///e:/SteamLibrary/steamapps/workshop/content/294100/RebornEvolvingMagic/TEST_GUIDE.md)





---

## 8. Arquitetura do Sistema de Habilidades e Técnicas (Marciais & Espirituais)

### 8.1 Matriz Tripla de Categorização
1. **Natureza do Recurso**:
   * **Técnicas Marciais**: Consomem **Vigor (SP)** e escalam seus efeitos e danos primariamente com **Força (STR)**, **Agilidade (AGI)** e **Constituição (CON)**.
   * **Feitiços da Alma**: Consomem **Energia da Alma (MP)** e escalam primariamente com **Condutividade (CND)** e **Sentidos (SEN)**.
   * **Técnicas Híbridas**: Consomem tanto SP quanto MP, unindo poder muscular e manipulação de éter cósmico.
2. **Afinidades Elementais & Leis Cósmicas**:
   * Vento (Cinético / Esquiva / Arrancada)
   * Terra (Gravidade / Rocha / Impacto Massivo / Densidade)
   * Fogo & Relâmpago (Penetração / Dano Térmico Explosivo)
   * Alma Pura & Éter (Dano espiritual direto ignorando armaduras físicas)
   * Espaço & Vácuo (Transposição instantânea / Cortes dimensionais)
3. **Grau de Estágio**:
   * Habilidades graduadas por classes: Mortal, Terra, Céu, Mestre, Grande Mestre, Lorde, Rei, Angélico, Ascendente.

### 8.2 Desbloqueio e Aquisição
* **Habilidades Fundamentais (Automáticas)**: Cada novo estágio alcançado desperta organicamente 2 habilidades base na Matriz do cultivador (1 Marcial e 1 Espiritual).
* **Habilidades Avançadas e Raras (Manuais de Técnicas)**:
  * Registradas em **Manuais de Técnicas** (`REM_Item_TechniqueManual`).
  * **Regra de Herança da Seita**: O manual **NÃO se destrói ao ser lido**! Ele permanece no acervo/biblioteca da colônia para ser estudado por qualquer colono que atinja os pré-requisitos de Estágio e Atributos.
  * **Obtenção Mista**:
    * Saque de invasores poderosos, chefes e cultivadores rivais.
    * Comércio com caravanas e mercadores celestiais.
    * Transcritos e forjados na colônia em uma **Mesa de Caligrafia e Transcrição da Alma** por colonos sábios com alto Intelecto e Condutividade.

### 8.3 Maestria e Proficiência das Técnicas (Uso Orgânico + Meditação Focada)
Cada habilidade dominada possui 4 Graus de Maestria:
1. **Iniciado (Grau 1)**: Base da técnica (100% custo, 100% dano).
2. **Adepto (Grau 2)**: -10% custo de SP/MP, +15% potência.
3. **Perito (Grau 3)**: -20% custo de SP/MP, +30% potência, -20% tempo de execução.
4. **Mestre da Técnica (Grau 4)**: -25% custo de SP/MP, +50% potência e aprimoramento de efeito secundário (área expandida, atordoamento aumentado ou perfuração extra).
* **Fontes de Proficiência**:
  * **Uso em Combate**: Cada execução da habilidade gera proficiência imediata.
  * **Foco na Meditação**: Na Aba "Alma", o jogador pode marcar 1 técnica como "Foco de Meditação"; enquanto o colono medita na Matriz, acumula proficiência contínua na técnica.

### 8.4 Rol das Habilidades Fundamentais dos 3 Primeiros Estágios
* **Classe Mortal (Tier 1)**:
  * **Disparo da Alma** (`REM_Ability_SoulBolt`): 20 MP, 25 dano direto, 10 XP. Escala com CND e SEN.
  * **Passo do Vento** (`REM_Ability_WindDash`): 25 SP, arrancada veloz de até 8 células, esquiva instantânea e reposicionamento. Escala com AGI.
* **Classe Terra (Tier 2)**:
  * **Corpo Telúrico** (`REM_Ability_EarthBody`): 40 SP, duração de 20s. Enrijece ossos e músculos com granito místico (+50% armadura física, imunidade a atordoamento). Possui toggle de autocast opcional. Escala com CON.
  * **Onda Elemental Telúrica** (`REM_Ability_EarthWave`): 45 MP, fende o solo em linha de 15 células causando dano de impacto e estilhaçamento de coberturas. 22.5 XP. Escala com CND e STR/SEN.
* **Classe Céu (Tier 3)**:
  * **Passo Celestial** (`REM_Ability_SkyStep`): 30 MP, transposição a vapor cósmico de até 30 células ignorando paredes. 15 XP.
  * **Lança Celestial** (`REM_Ability_SkyLance`): 35 MP, 50 dano perfurante e 2s stun. 17.5 XP. Escala com CND e SEN.
  * **Palma Celestial do Vácuo** (`REM_Ability_VoidPalm`): 50 SP, cone de 12 células de ar comprimido cortante, rasgando armaduras e empurrando inimigos. 25 XP. Escala com STR e AGI.

### 8.5 Interface e Gestão na Aba "Alma"
* Sub-abas internas na Aba "Alma":
  * `[Geral & Atributos]`: Estágio, nível, barras SP/MP/XP, atributos STR/CON/AGI/SEN/CND com botões `[+]` e galeria da Pérola Mágica (se portador).
  * `[Grimório de Técnicas]`: Lista de técnicas dominadas com ícone, nome, custo de SP/MP, dano estimado, barra de progresso de Maestria (Iniciado $\rightarrow$ Mestre) e botão de estrela para fixar como "Foco de Meditação".
* **Ativação em Combate**:
  * Comandos diretos ao recrutar o colono (Draft), diferenciados por bordas verdes (Marcial/SP) e azuis (Espiritual/MP).
  * Sem limites artificiais de slots equipados; o limite tático é o Vigor e a Energia da Alma.
  * Botão de autocast opcional para posturas defensivas.


---

## 9. Especificação Detalhada das 10 Novas Habilidades & Técnicas

### 9.1 Classe Mortal (Tier 1)
1. **Flecha de Fogo** (`REM_Ability_FireArrow`):
   * **Tipo**: Espiritual (Éter Ígneo) | **Custo**: 15 MP | **XP**: +7.5 XP.
   * **Efeito**: Projeta um dardo concentrado de fogo espiritual que causa 20 de dano de queimadura e incendeia imediatamente as vestes/pele do alvo, forçando pânico de chamas.
2. **Curar** (`REM_Ability_Heal`):
   * **Tipo**: Espiritual (Éter Vital) | **Custo**: 25 MP (Graus 1-3) / 60 MP (Grau 4) | **XP**: +12.5 XP.
   * **Alcance**: Em si mesmo ou aliado em até 12 células.
   * **Efeito**:
     * Graus 1 a 3: Cura instantaneamente de 30 a 60 HP distribuídos nos ferimentos mais graves, fecha sangramentos e alivia dores.
     * Grau 4 (Mestre): Canalização cirúrgica capaz de regenerar partes do corpo destruídas (dedos, olhos, membros) e curar cicatrizes permanentes.
3. **Pressa** (`REM_Ability_Haste`):
   * **Tipo**: Marcial (Vigor Muscular) | **Custo**: 25 SP | **XP**: +12.5 XP.
   * **Duração**: 30 segundos.
   * **Efeito**: Acelera o fluxo sanguíneo e reflexos corporais, concedendo +50% de velocidade de movimento (`MoveSpeed`) e +40% de velocidade de trabalho global (`WorkSpeedGlobal`).

### 9.2 Classe Terra (Tier 2)
4. **Pele de Pedra** (`REM_Ability_StoneSkin`):
   * **Tipo**: Marcial (Endurecimento Físico) | **Custo**: 30 SP | **XP**: +15 XP.
   * **Duração**: 30 segundos (com toggle opcional de autocast em combate).
   * **Efeito**: Solidifica a epiderme em uma camada de rocha densa: +60% armadura cortante (`Sharp`), +50% contundente (`Blunt`), +40% térmica (`Heat`). Penalidade: -20% de velocidade de movimento.
5. **Regeneração Acelerada** (`REM_Ability_AcceleratedRegen`):
   * **Tipo**: Híbrido (Vigor e Éter) | **Custo**: 40 MP | **XP**: +20 XP.
   * **Duração**: 1 dia in-game (24 horas = 60.000 ticks).
   * **Efeito**: Multiplica por 5x a taxa de cicatrização natural de ferimentos (`InjuryHealingFactor` +400%), fechando feridas a cada segundo e reduzindo perda sanguínea continuamente.
6. **Raio de Gelo** (`REM_Ability_FrostRay`):
   * **Tipo**: Espiritual (Éter Glacial) | **Custo**: 30 MP | **XP**: +15 XP.
   * **Efeito**: Feixe congelante de longo alcance que causa 30 de dano frio/frostbite e aplica +40% de severidade imediata de hipotermia grave no alvo, colapsando a velocidade e manipulação do inimigo.
7. **Escudo Psiônico** (`REM_Ability_PsionicShield`):
   * **Tipo**: Espiritual (Barreira de Éter) | **Custo**: 45 MP | **XP**: +22.5 XP.
   * **Duração**: **Infinita** até o esgotamento total da integridade do escudo.
   * **Efeito**: Envolve o cultivador em uma cúpula unilateral com 150 HP de durabilidade (+ bônus de Condutividade). Bloqueia projéteis e feitiços vindos de fora, mas **permite ao cultivador atirar e conjurar feitiços de dentro para fora**.

### 9.3 Classe Céu (Tier 3)
8. **Arco Voltaico** (`REM_Ability_ChainLightning`):
   * **Tipo**: Espiritual (Relâmpago Celeste) | **Custo**: 45 MP | **XP**: +22.5 XP.
   * **Efeito**: Descarga elétrica colossal de 45 de dano direto + 2.5s de paralisia/atordoamento. Salta em cadeia para inimigos próximos em um raio de 5 células.
   * **Escala de Saltos**: O número de saltos cresce com a Maestria (2 saltos no Grau Iniciado até 5 saltos no Grau Mestre), com decaimento leve de dano entre saltos (100% $\rightarrow$ 80% $\rightarrow$ 60%).
9. **Teleporte** (`REM_Ability_Teleport`):
   * **Tipo**: Espiritual / Espacial | **Custo**: 40 MP | **XP**: +20 XP.
   * **Efeito**: Transpõe instantaneamente o cultivador para qualquer célula selecionada no mapa, atravessando montanhas e estruturas.
   * **Alcance por Maestria**:
     * Grau 1 (Iniciado): 20 células (alcance mínimo).
     * Grau 2 (Adepto): 35 células.
     * Grau 3 (Perito): 50 células.
     * Grau 4 (Mestre): **Mapa Inteiro** (qualquer célula navegável do mapa local).
10. **Benção dos Céus** (`REM_Ability_HeavensBlessing`):
    * **Tipo**: Híbrida / Suprema | **Custo**: 80 MP + 40 SP | **XP**: +40 XP.
    * **Efeito Combinado Quíntuplo**:
      * Gera um Escudo Psiônico Supremo de **750 HP** (duração infinita até ser quebrado).
      * Concede por 45 segundos: **Regeneração Quíntupla** (fecha ferimentos graves em segundos) e **Pele de Pedra Suprema** (+100% de armadura cortante/contundente e extrema absorção de impacto), **SEM nenhuma perda de mobilidade**!

---

## 10. Nova Escala de Poder & Recálculo de Status dos Atributos

### 10.1 Escala Ampliada de Atributos e Impacto Biológico / Combate:
1. **Força (STR)**:
   * Dano Melee: `+1.5%` por ponto linear (`MeleeDamageFactor` até 4.0x).
   * Carga Titânica: `+3.0 kg` por ponto (carrega árvores e suprimentos inteiros).
   * Penetração de Armadura: $0.80 \times \left(\frac{\text{STR}}{\text{STR} + 150}\right) \times 100\%$.
   * Atordoamento Melee: chance de desequilibrar ou atordoar oponentes por força física.
   * Velocidade de Mineração e Construção: `+1.2%` e `+1.0%` por ponto.
2. **Constituição (CON)**:
   * Vida Máxima: `+1.0%` por ponto linear (`HealthScale` até 3.0x).
   * Mitigação de Dano Passivo: até $70\%$ de redução com curva assintótica suave.
   * Amplitude Térmica: `ComfyTemperatureMin` ($-0.5^\circ\text{C}$/pt) e `ComfyTemperatureMax` ($+0.5^\circ\text{C}$/pt), suportando frio ou calor extremos.
   * Imunidade a Doenças e Toxinas: `+1.0%` e `+0.8%` por ponto.
   * Fome e Sono Reduzidos: consome até $-75\%$ menos comida e descansa duas vezes mais rápido.
3. **Agilidade (AGI)**:
   * Velocidade de Movimento: `+0.06 c/s` linear contínuo **SEM TETO**.
   * Esquiva Fantasmagórica de Projéteis: desvia de tiros balísticos com feedback `"ESQUIVOU!"`.
   * Cadência de Golpes Melee: reduz o cooldown corporal em até $-70\%$.
   * Tarefas Manuais Finas e Medicina: `+1.0%` de velocidade de trabalho por ponto.
4. **Sentidos (SEN)**:
   * Precisão de Tiro: `+0.4%` por ponto.
   * Acertos Críticos Físicos e Mágicos: chance de até $60\%$ e multiplicador de $1.5\text{x}$ até $3.0\text{x}$ de dano.
   * Velocidade de Aprendizado Global: `+0.8%` por ponto.
   * Estabilidade Mental: reduz em $-0.2\%$ por ponto o limiar de colapso mental.
5. **Condutividade (CND)**:
   * Teto de Energia da Alma (MP): $+3.0\%$ composto por ponto sobre o valor anterior ($\text{Base MP} \times 1.03^{\text{CND}}$).
   * Dano de Feitiços: `+1.5%` por ponto linear.
   * Potência de Escudos e Curas: `+1.5%` por ponto linear.
   * Regeneração Passiva de Mana: `+0.8%` da taxa por ponto.
   * Sensibilidade Psíquica e Afinidade Natural: `+1.0%` e `+0.8%` por ponto.

---

## 11. Projéteis Nativos de Cultivo & Bloqueio Hermético por Escudos

### 11.1 Arquitetura sem Bibliotecas Externas (Zero JecsTools)
* As habilidades mágicas de ataque à distância deixaram de aplicar dano instantâneo tipo "hitscan direto", passando a lançar projéteis físicos balísticos voadores nativos (`Verse.Projectile`).
* Cada projétil possui sua própria classe derivada de `Projectile_CultivationSpell`, com velocidade balística, sprite com efeito de brilho (`TransparentPostLight`), e lógica de impacto.

### 11.2 Projéteis Implementados
1. **Disparo da Alma (`REM_Proj_SoulBolt`)**: Dano 25 Burn, Vel 65, textura de pulso de carga ciano.
2. **Flecha de Fogo (`REM_Proj_FireArrow`)**: Dano 20 Flame, Vel 55, dardo incandescente; inflama o alvo se não bloqueado.
3. **Raio de Gelo (`REM_Proj_FrostRay`)**: Dano 30 Frostbite, Vel 70, feixe glacial translúcido; aplica hipotermia severa se não bloqueado.
4. **Lança Celestial (`REM_Proj_SkyLance`)**: Dano 50 Burn perfurante, Vel 85, lança celestial brilhante; atordoa o alvo se não bloqueado.
5. **Arco Voltaico (`REM_Proj_ChainLightning`)**: Dano 45 Burn elétrico, Vel 75; atordoa o alvo primário e ricocheteia em cadeia para até 5 inimigos em um raio de 5 células, conferindo atordoamento apenas se o escudo do alvo secundário não absorver o impacto.

### 11.3 Correção Hermética de Efeitos Secundários Sob Escudo
* **Problema Original**: Quando um alvo com escudo (seja *Escudo Psiônico* ou cinto de escudo vanilla) sofria um ataque mágico, o escudo absorvia o dano numérico, mas efeitos secundários como fogo (`TryAttachFire`), hipotermia (`Hypothermia`) e atordoamento (`StunFor`) eram aplicados diretamente na carne da criatura.
* **Solução Definitiva**:
  * No método `Impact(Thing hitThing, bool blockedByShield)` do `Projectile_CultivationSpell`, é verificado se o projétil foi bloqueado pelo escudo do jogo (`blockedByShield == true`).
  * Em seguida, ao aplicar dano (`hitThing.TakeDamage(dinfo)`), o resultado é inspecionado: `bool absorbed = (result == null || result.totalDamageDealt <= 0.01f || result.deflected)`.
  * **O método virtual `OnDamagedTarget(...)` (responsável por atear fogo, hipotermia e atordoamento) só é executado se `!absorbed`**.
  * Portanto, enquanto o escudo tiver durabilidade e absorver o impacto, **nenhum efeito adverso vaza para o usuário protegido**.

---

## 12. Motor de Animações, VFX em Camadas & Assets Proprietários (Inspiração RoM & Kure's)

### 12.1 Arquitetura Nativa e Independente de DLCs/Dependências
* Inspirado pelas técnicas visuais de **A RimWorld of Magic (RoM)** e **Kure's Class Expansion Pack**, todo o pipeline de animações é implementado nativamente usando apenas **RimWorld Core** (`Verse.FleckCreationData`, `ShaderDatabase.MoteGlow`, `PawnFlyer`, `UnityEngine.Graphics.DrawMesh`), sem dependência de `TorannMagic`, `AbilityUser`, `JecsTools` ou DLCs (Royalty/Biotech).
* **18 sprites proprietários em alta resolução** gerados em escala de cinza/branco com bloom e alfa suave para tingimento dinâmico via `MoteGlow`:
  * `Textures/Motes/RebornMagic/` (`RuneCircle`, `SwirlVortex`, `ShockwaveRing`, `GlowOrb`, `SoftSpark`, `EnergyStreak`, `LightningBeam`, `HexShieldDome`, `IceCrystal`, `FlameWisp`, `StoneShard`, `HealingPetal`, `PowerBeam`).
  * `Textures/Projectiles/RebornMagic/` (`Proj_SoulBolt`, `Proj_FireArrow`, `Proj_FrostRay`, `Proj_SkyLance`, `Proj_ChainLightning`).

### 12.2 Camadas Visuais, Raios em Malha, Flyers e Cúpula de Escudo
1. **Rastros Contínuos e Explosões Multi-Camada em Projéteis (`Projectiles_Cultivation.cs`)**:
   * Todos os 5 projéteis emitem rastro por tick em voo (orbes luminosos, filetes direcionais, chamas `FlameWisp` ou cristais `IceCrystal`), arranjo rúnico giratório na saída e explosão multi-camada no impacto (`ShockwaveRing` + `SwirlOut` + estilhaços + som). Acertos críticos disparam clarão dourado e runa rápida.
2. **Arcos Elétricos Dinâmicos (`RebornLightningArc`)**:
   * O *Arco Voltaico* e a *Absorção de Alma* geram malhas procedurais poligonais bifurcadas com núcleo branco-azulado de alta intensidade e jitter dinâmico (`Graphics.DrawMesh`), conectando conjurador, alvo primário e todos os ricochetes em cadeia.
3. **Deslocamento Físico Fluido (`RebornDashFlyer` / `RebornSkyStepFlyer`)**:
   * *Passo do Vento* e *Passo Celestial* lançam o colono via `PawnFlyer` customizado (`REM_Flyer_WindDash` e `REM_Flyer_SkyStep`), deixando pós-imagens, rastros direcionais, partículas no ar e onda de choque/runa na aterrissagem (preservando seleção e status *Drafted*), enquanto *Teleporte* conjura portais duplos de implosão/explosão (`SwirlIn`/`SwirlOut`).
4. **Cúpula Hexagonal Translúcida & Auras Contínuas (`RebornVFX_Patches.cs`)**:
   * *Escudo Psiônico* desenha em tempo real uma cúpula hexagonal giratória translúcida ao redor do portador (mudando de cor conforme HP restante e piscando em branco/ciano ao absorver dano).
   * *Bênção dos Céus* exibe arranjo rúnico dourado sob os pés e orbes orbitais; *Pele de Pedra*, *Pressa* e *Regeneração Acelerada* emitem partículas orbitais/ascendentes periódicas (`FleckAttachLink` + `orbitSpeed`).
5. **Rituais e Pilares de Ascensão (`PlayAscensionPillar` / `PlaySoulSiphon`)**:
   * *Batismo das Leis*, *Bênção dos Céus*, *Despertar da Matriz* e *Fusão da Pérola Mágica* invocam pilar vertical celestial (`PowerBeam`), arranjo rúnico duplo e dupla onda de choque com trovão.

# Core — Brief de Arquitetura

Documento de referência do pacote fundação. Sem dependências externas — todo o ecossistema Concept Factory depende deste pacote; este não depende de nada.

---

## 1. Princípio central

**Core é genérico por definição.** Qualquer coisa que vá aqui precisa passar no teste: "um pacote de Melee, um pacote de Puzzle e um pacote de UI poderiam usar isso sem saber o que são os outros dois?" Se a resposta for não, o código não pertence ao Core.

---

## 2. Módulos implementados

### 2.1 Sistemas de comunicação

- **`EventBus`** — pub/sub desacoplado. Emissores e ouvintes não se conhecem. Use quando o emissor não precisa saber se alguém está ouvindo.
- **`CallbackHub`** — callbacks nomeados com cleanup automático. Use quando quem registra precisa garantir deregistro seguro (ex.: objetos com ciclo de vida curto).

### 2.2 Fluidos (`Fluids/`)

Sistema compartilhado entre `ConceptFactory.Interactions` (PortableFluidContainer) e `ConceptFactory.Vehicles` (combustível, óleo, líquido de arrefecimento).

- `Fluid` / `FluidMixture` — tipos de dados puros, imutáveis.
- `FluidRegistry` — registro global de fluidos conhecidos do projeto.
- `FluidContainer` / `FluidReservoir` — componentes físicos com volume rastreado.
- `FluidSettings` — asset de configuração global de fluidos.

**Regra**: a definição do que é um fluido e como ele se mistura mora aqui. O que se faz com o fluido (encher um tanque de carro, usar como combustível) mora nos pacotes específicos.

### 2.3 Condição e Saúde (`Condition/`)

- `ObjectCondition` — estado genérico de saúde/dano para qualquer GameObject.
- `IHasHealth` — interface: qualquer componente com barra de vida.
- `HealthThresholds` — eventos disparados quando a vida cruza limiares configuráveis.

**Nota (2026-10)**: `Condition/` e `Fluids/` foram adicionados recentemente — estavam fora do git na última verificação. Confirmar que estão rastreados antes de publicar.

### 2.4 Addressables

- `AddressablesManager` — wrapper sobre Unity Addressables com lifetime explícito. Evita leaks de referência ao centralizar load/unload assíncrono.

### 2.5 Helpers

Utilitários de propósito geral que não justificam um módulo próprio:

| Tipo | Arquivo |
|---|---|
| Singleton MonoBehaviour | `TSingleton<T>` |
| Multi-delegate seguro | `ActionManager` |
| Operação async com progresso | `ProgressTask` |
| Textura remota em cache | `WebTexture` |
| HTTP leve | `NetworkHelper` |
| JsonUtility wrappers | `JsonExtensions` |
| Discord webhook | `DiscordUtils` |
| Extensões variadas | `IOExtensions`, `ColorExtensions`, `AsyncOperationExtensions` |

### 2.6 Input (`Input/`)

**`IInputCallBacks` e `InputMonitor` são input de toque/gesture mobile** (OnTouch, OnPinching, OnDragging). **Não são** input de gameplay de movimento — não confundir com `IMovementInputProvider` do pacote Movement. São conceitos completamente distintos.

### 2.7 Drawers / Atributos

Inspector-only. Zero impacto no runtime:

- `[PathPicker]` — seletor de arquivo/pasta no Inspector.
- `[VectorRange]` — vetor com range limitado.
- `[ReadOnly]` — campo serializado não editável no Inspector.

---

## 3. O que NÃO entra aqui

- Qualquer referência a animações, Animator, clipes — é conteúdo de pacote específico.
- Lógica de câmera, movimento, input de gameplay — pacotes próprios.
- Assets de cena ou prefabs — Core não tem conteúdo de cena, só código e assets de configuração.
- Referências a outros pacotes `com.conceptfactory.*` — Core não pode referenciar nenhum deles.

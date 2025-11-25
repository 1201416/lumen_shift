# Como Configurar Colaboração no Unity

## Problema
O colega não consegue aceder ao projeto no Unity mesmo fazendo "add from repository". É necessário criar uma organização e adicionar o colega para conseguir listar o projeto.

## Solução: Criar Organização Unity e Adicionar Colaborador

### Passo 1: Criar uma Organização Unity

1. **Aceder ao Unity Dashboard**
   - Vai a: https://dashboard.unity3d.com
   - Faz login com a tua conta Unity

2. **Criar Nova Organização**
   - No canto superior direito, clica no teu nome/perfil
   - Seleciona "Organizations" ou "Create Organization"
   - Clica em "Create New Organization"
   - Preenche os dados:
     - **Nome da Organização**: (ex: "LumenShift" ou o nome que preferires)
     - **Plano**: Escolhe o plano adequado (Free tier permite colaboração básica)
   - Confirma a criação

### Passo 2: Criar/Conectar Projeto à Organização

#### Opção A: Se estás a usar Unity Cloud Build ou Unity Collaborate

1. **No Unity Editor:**
   - Abre o projeto no Unity
   - Vai a `Edit > Project Settings > Services` (ou `Window > General > Services`)
   - Faz login com a tua conta Unity se ainda não estiveres logado
   - Seleciona a organização que acabaste de criar
   - Liga o projeto à organização

2. **No Unity Dashboard:**
   - Vai a https://dashboard.unity3d.com
   - Seleciona a tua organização
   - Clica em "Projects" ou "Cloud Build"
   - Cria um novo projeto ou conecta o projeto existente

#### Opção B: Se estás a usar Git (como parece ser o caso)

Se o projeto está no Git/GitHub/GitLab, podes ainda assim usar Unity Cloud Build:

1. **Configurar Unity Cloud Build:**
   - No Unity Dashboard, dentro da tua organização
   - Vai a "Cloud Build"
   - Clica em "Create New Build Target"
   - Conecta o repositório Git:
     - Seleciona o serviço (GitHub, GitLab, Bitbucket, etc.)
     - Autoriza o acesso ao repositório
     - Seleciona o branch (ex: `main`)
   - Configura as plataformas de build necessárias

### Passo 3: Adicionar Colaborador à Organização

1. **No Unity Dashboard:**
   - Vai a https://dashboard.unity3d.com
   - Seleciona a tua organização
   - Vai ao separador "Members" ou "Team"
   - Clica em "Add Member" ou "Invite Member"
   - Introduz o email do teu colega (deve ser o email associado à conta Unity dele)
   - Seleciona o nível de permissão:
     - **Owner**: Acesso total
     - **Manager**: Pode gerir projetos e membros
     - **Developer**: Pode aceder e trabalhar nos projetos
   - Envia o convite

2. **O teu colega precisa de:**
   - Aceitar o convite por email
   - Fazer login na conta Unity com o email do convite
   - A organização aparecerá na lista dele

### Passo 4: O Colega Acede ao Projeto

Depois de ser adicionado à organização, o teu colega pode:

1. **No Unity Editor:**
   - Abre Unity Hub
   - Clica em "Open" ou "Add"
   - Seleciona "Add project from version control"
   - Faz login com a conta Unity
   - Seleciona a organização
   - O projeto deve aparecer na lista

2. **Ou via Unity Dashboard:**
   - Vai a https://dashboard.unity3d.com
   - Seleciona a organização
   - Vê a lista de projetos
   - Clica no projeto para ver detalhes

## Notas Importantes

- **Unity ID**: O colega precisa de ter uma conta Unity (pode criar em https://id.unity.com)
- **Email**: O email usado no convite deve ser o mesmo da conta Unity do colega
- **Permissões**: Garante que o colega tem permissões adequadas para aceder ao projeto
- **Git vs Unity Services**: Se estás a usar Git, o colega pode clonar diretamente, mas para usar Unity Cloud Build ou Collaborate precisa da organização

## Alternativa: Usar apenas Git

Se não precisares de Unity Cloud Build ou Collaborate, o colega pode simplesmente:
1. Clonar o repositório Git diretamente
2. Abrir o projeto no Unity normalmente
3. Não precisa de organização para isso

Mas se o problema é que ele não consegue "listar o projeto" no Unity Hub, então provavelmente precisas mesmo da organização.

## Troubleshooting

- Se o projeto não aparece: Verifica que o projeto está ligado à organização correta
- Se o convite não chega: Verifica o spam e confirma o email
- Se não consegue fazer login: Verifica que está a usar a conta Unity correta



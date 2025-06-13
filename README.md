# Aplicação CashFlow

## Introdução

Este projeto foi feito como teste para um processo seletivo. O enunciado do teste se encontra em [docs/TestStatement.pdf](docs/TestStatement.pdf).



## Problema proposto

Como mencionado na Introdução, o enunciado completo se encontra no arquivo TestStatement.pdf, entretanto, resumindo de forma geral, trata-se de uma aplicação de fluxo de caixa, onde há uma API de lançamentos de entradas (débitos e créditos), e uma outra API, onde é gerado um relatório de totalização por dia de consulta. Esta aplicação tem requisitos não funcionais de alta disponibilidade e resiliência.



## Tecnologias utilizadas

Para a construção do projeto, foram utilizados:
- .Net 9.0, para a API Web;
- Postgresql, como Banco de Dados;
- Seq, como servidor de logs estruturados;
- XUnit, como biblioteca de testes unitários;
- Reqnroll, como framework BDD;
- K6, como ferramenta de testes de carga;
- Docker para conteinerização.



## Como executar o projeto

O projeto pode ser executado de pelo menos três formas: localmente por linha de comando, pelo Visual Studio Code e pelo Docker Compose. Também é possível executar por outras ferramentas, como Visual Studio e Rider, mas somente as três primeiras serão tratadas aqui.

### Localmente por linha de comando

#### Requisitos:

- SDK do .Net 9.0;
- Banco de dados Postgresql;
- Seq;
- K6.

#### API:

1. Ajuste o arquivo appsettings.Development.json para as configurações atuais de servidor do Posgresql e do Seq;
2. Abra o terminal de sua preferência na pasta raiz do projeto e digite: `dotnet run --project ./src/CashFlow.Api`.
3. Acesse `https://localhost:7027/`.

#### Testes Unitários e Comportamentais:

Abra o terminal de sua preferência na pasta raiz do projeto e digite:

- Para rodar os testes unitários:
`dotnet test ./tests/CashFlow.Api.UnitTests/CashFlow.Api.UnitTests.csproj`
- Para rodar os testes comportamentais:
`dotnet test ./tests/CashFlow.FeatureTests/CashFlow.FeatureTests.csproj`

#### Testes de carga:

Primeiro, certifique-se que sua API está rodando, e que o banco de dados está limpo. Caso hajam registros, os testes de lógica não passarão. Ajuste o valor de K6_BASE_URL_ENV de acordo com a URL da Api. Depois, abra o terminal de sua preferência na pasta `tests/PerformanceTests/` do projeto e digite:

- Para o teste de carga: 
`k6 run scripts/script-load.js --insecure-skip-tls-verify --out json=results/test-load.json -e K6_BASE_URL=${K6_BASE_URL_ENV:-https://localhost:7027}`
- Para o teste de lógica: 
`k6 run scripts/script-logic.js --insecure-skip-tls-verify --out json=results/test-logic.json -e K6_BASE_URL=${K6_BASE_URL_ENV:-https://localhost:7027}`

Veja os resultados no console ou na pasta `results`.


### Visual Studio Code

#### Requisitos:

- Visual Studio Code;
- Extensão C#;
- Extensão C# Dev Kit;
- SDK do .Net 9.0;
- Banco de dados Postgresql;
- Seq.

#### API:

1. Ajuste o arquivo appsettings.Development.json para as configurações atuais de servidor do Posgresql e do Seq;
2. Na aba "Executar e Depurar", selecione `.NET Watch DevKit` para depurar utilizando a extensão "C# Dev Kit", ou `.NET Run CoreClr` para executar diretamente;
3. Aperte o Play, ou F5;
4. Acesse `https://localhost:7027/`.

#### Testes Unitários e Comportamentais:

Utilize normalmente a aba de testes do Visual Studio Code, inserida pela extensão "C# Dev Kit".

#### Testes de carga:

Não há configurações para rodar diretamente pelo Visual Studio Code.


### Docker

#### Requisitos:

- Docker;
- Docker Compose;
- NodeJS (opcional).

#### API:

Abra o terminal de sua preferência na pasta raiz do projeto e digite:
```
# (Para evitar problema de cache)
docker build -t cashflow-api -f src/CashFlow.Api/Dockerfile .
docker-compose -f docker-compose-services.yml -f docker-compose.yml up
```
O projeto estará acessível na url `http://localhost:8090/`.

#### Testes Unitários e Comportamentais:

Não se aplica, os testes não rodam via docker.

#### Testes de carga:

Primeiro, certifique-se que sua API está rodando, e que o banco de dados está limpo. Caso hajam registros, os testes de lógica não passarão. Ajuste o valor de K6_BASE_URL_ENV de acordo com a URL da Api. Depois, abra o terminal de sua preferência na pasta `tests/PerformanceTests/` do projeto e digite:

- Para o teste de carga: 
`docker run --rm -i --user $(id -u):$(id -g) -v $(pwd):/src --workdir /src -e K6_BASE_URL=${K6_BASE_URL_ENV:-https://localhost:7027} --network=host grafana/k6 run /src/scripts/script-load.js --insecure-skip-tls-verify --out json=/src/results/test-load.json`
- Para o teste de lógica: 
`docker run --rm -i --user $(id -u):$(id -g) -v $(pwd):/src --workdir /src -e K6_BASE_URL=${K6_BASE_URL_ENV:-https://localhost:7027} --network=host grafana/k6 run /src/scripts/script-logic.js --insecure-skip-tls-verify --out json=/src/results/test-logic.json`

Como alternativa mais simples, caso você possua o NodeJS instalado, você não precisa executar o `npm install`, pode apenas executar os scripts:
- Para o teste de carga: `npm run load`
- Para o teste de lógica: `npm run logic`

Veja os resultados no console ou na pasta `results`.


### Observação:

Para todos os ambientes, exceto produção, as migrações serão aplicadas automaticamente ao executar o projeto pela primeira vez.

### Dica:

Para limpeza do Banco de Dados, para a correta execução do teste de lógica do K6, conecte-se ao Banco de Dados utilizando o cliente de sua preferência, e execute o seguinte script:
```sql
delete from "DailyConsolidated";
delete from "Entries";
```

Caso não queira ter que acessar um cliente do Postgresql para limpar a tabela, você pode fazer dois ajustes no código: Alterar baseDate para uma data não utilizada, ou comentar a função setup(), pois é ela quem verifica os valores iniciais.


## Explicação da solução

### Tecnologias

- .Net 9.0 - Última versão da plataforma alvo do teste;
- Postgresql - Banco de dados leve, gratuito e eficiente, ideal para diversos tipos de projeto;
- Seq - solução simples e fácil de utilizar em container para armazenamento de logs;
- XUnit - biblioteca mais utilizada para testes unitários em .Net, usada inclusive para testar o próprio framework oficial;
- Reqnroll - sucessor do SpecFlow, que foi descontinuado, ambas versões do Cucumber para .Net, principal ferramenta de BDD do mercado utilizando o Gherkin;
- K6 - apesar de haverem ferramentas em .Net para testes de carga, o K6 é conhecido por ser bastante robusto e de fácil utilização para esse propósito. Também foi escolhido para apresentar flexibilidade em outras stacks;
- Docker - principal ferramenta de conteinerização.

### Arquitetura

Embora muitos testes esperem uma arquitetura complexa, e esta seja a solução normalmente entregue, eu acredito que é um erro. Primeiro, porque arquiteturas complexas existem para resolver problemas complexos, então, aplicá-las a problemas simples, seria uma solução errada. Segundo, porque devemos evitar overengineering, a menos que tenhamos um bom motivo, e fazer um overengineering no teste pode significar fazer overengineering na prática do dia-a-dia. E, terceiro, porque dar soluções simples é normalmente mais difícil, mas de alto valor. Saber desenhar soluções sob medida sem precisar de receitas de bolo é uma qualidade fundamental de um arquiteto.

O design da aplicação se assemelha ao MVC. Os endpoints funcionam como as controllers, orquestrando as chamadas entre os serviços DAO e os modelos.

Como padrões de qualidade, a aplicação respeita o SOLID, além de a arquitetura refletir os princípios KISS e YAGNI. A união de ambos os serviços em um único projeto também reflete o princípio DRY.

Sobre a questão da independência entre os serviços de lançamento e relatório, à primeira vista pode parecer que os serviços estão acoplados, mas não é bem assim. Um dos requisitos do teste é que o serviço de lançamentos não deve ficar indisponível caso o de relatórios caia. Isso pode ser facilmente obtido fazendo o deploy da aplicação como serviços separados, e utilizando um serviço de Gateway para direcionar as chamadas. Apesar de não ter sido aplicado no teste, isso é muito fácil de configurar em serviços como o Ocelot, por exemplo. Isso é possível graças à aplicação ser stateless, permitindo escalabilidade horizontal.

Uma arquitetura mais simples possibilitou tempo para investir em uma boa cobertura de testes automatizados, testes comportamentais e de carga, além da configuração do Docker. Em um cenário real, soluções adequadas também economizam tempo e dinheiro de nossos clientes.

Quanto ao cálculo dos totalizadores, foi utilizada uma function do Postgresql para lidar com o problema da concorrência do cálculo, onde somente as novas entradas precisam ser calculadas, e sem ter que criar um serviço extra, mantendo a aplicação escalável horizontalmente. Para retornar a lista de valores do dia, foi utilizada uma lógica de listagem de todos os valores calculados pelos totalizadores, e os índices na tabela permitiram uma busca eficiente, sem necessidade de cache, o que foi avaliado através dos testes de carga, porém esta medida poderia ser reavaliada no futuro.


## O que eu gostaria de ter feito a mais?

Muita coisa poderia ter sido adicionada. Seguem algumas:

- Integração com o Kubernetes, para realização dos testes de carga ser mais fiel ao cenário de produção e poder refletir melhor a disponibilidade;
- API Gateway, como Ocelot, para separar os serviços, como se fossem aplicações distintas;
- Autorização, para separar os acessos das API's;
- Melhoria na observabilidade com telerimetria, como com o OpenTelemetry, por exemplo;
- Diagramas UML que ajudem a explicar de uma forma geral a solução.
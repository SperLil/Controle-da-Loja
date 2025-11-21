
--  Cria o banco de dados 
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BazarDB')
BEGIN
    CREATE DATABASE BazarDB;
END
GO

--  Usa o banco de dados criado
USE BazarDB;
GO

--  Cria a tabela principal: Produtos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Produtos' and xtype='U')
BEGIN
    CREATE TABLE Produtos (
        ProdutoID INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Preco DECIMAL(10, 2) NOT NULL,
        Quantidade INT NOT NULL,
        CaminhoImagem NVARCHAR(MAX) NULL
    );
END
GO
--  Cria a nossa nova tabela: Vendas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vendas' and xtype='U')
BEGIN
    CREATE TABLE Vendas (
        -- ID único da Venda
        VendaID INT PRIMARY KEY IDENTITY(1,1),
        
        -- Data/Hora em que a venda foi registada
        -- (DEFAULT GETDATE() preenche automaticamente com a data e hora atuais)
        DataVenda DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- ID do produto que foi vendido (a chave estrangeira)
        ProdutoID INT,
        
        -- Quantos itens foram vendidos
        QuantidadeVendida INT NOT NULL,
        
        -- Qual foi o valor total (Preço * Quantidade)
        ValorTotal DECIMAL(10, 2) NOT NULL,
        
        -- A ligação que garante que não podemos vender um produto que não existe
        FOREIGN KEY (ProdutoID) REFERENCES Produtos(ProdutoID)
    );
END
GO

PRINT 'Tabela [Vendas] criada com sucesso!';

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' and xtype='U')
BEGIN
    CREATE TABLE Clientes (
        ClienteID INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Telefone NVARCHAR(20) NULL -- (Opcional, mas bom para o fiado)
    );
END
GO

-- 5. ALTERA a tabela Vendas para adicionar o Cliente
-- Primeiro, verifica se a coluna ClienteID ainda NÃO existe
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'dbo.Vendas') 
               AND name = 'ClienteID')
BEGIN
    -- Adiciona a nova coluna ClienteID
    ALTER TABLE Vendas
    ADD ClienteID INT NULL,  -- (NULL = "Nulo", permitindo vendas anônimas/sem cliente)
    
    -- Adiciona a Chave Estrangeira ligando Vendas -> Clientes
    CONSTRAINT FK_Vendas_Clientes FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID);
    
    PRINT 'Tabela [Vendas] alterada para incluir ClienteID.';
END
GO

-- 6. ALTERA a tabela Vendas para adicionar o Status do Pagamento (Para o Fiado)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'dbo.Vendas') 
               AND name = 'StatusPagamento')
BEGIN
    ALTER TABLE Vendas
    ADD StatusPagamento NVARCHAR(50) NOT NULL DEFAULT 'Pago'; -- (O padrão é "Pago")
    
    PRINT 'Tabela [Vendas] alterada para incluir StatusPagamento.';
END
GO

PRINT 'Módulo de Clientes e Fiado adicionado ao banco de dados!';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Vendas') AND name = 'ProdutoID')
BEGIN
    PRINT 'Removendo colunas antigas da tabela Vendas...';

    -- Primeiro, remove a chave estrangeira (FK) que liga Vendas -> Produtos
    ALTER TABLE Vendas
    DROP CONSTRAINT FK__Vendas__ProdutoI__37A5467C; -- NOTA: O nome da sua FK pode ser diferente!
    
    -- Agora, remove as colunas que não pertencem mais ao "Recibo"
    ALTER TABLE Vendas
    DROP COLUMN ProdutoID,
                QuantidadeVendida,
                ValorTotal;
END
GO


--  Cria a tabela ItensVenda (o "Carrinho")
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ItensVenda' and xtype='U')
BEGIN
    CREATE TABLE ItensVenda (
        ItemVendaID INT PRIMARY KEY IDENTITY(1,1),
        
        -- A ligação com o "Recibo" (Mestre)
        VendaID INT NOT NULL, 
        
        -- A ligação com o Produto
        ProdutoID INT NOT NULL,
        
        Quantidade INT NOT NULL,
        PrecoUnitario DECIMAL(10, 2) NOT NULL, -- Guarda o preço no momento da venda

        -- Chaves Estrangeiras
        FOREIGN KEY (VendaID) REFERENCES Vendas(VendaID),
        FOREIGN KEY (ProdutoID) REFERENCES Produtos(ProdutoID)
    );
    PRINT 'Tabela [ItensVenda] criada com sucesso.';
END
GO
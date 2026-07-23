
--  Cria o banco de dados 
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BazarDB')
BEGIN
    CREATE DATABASE BazarDB;
END
GO

--  Usa o banco de dados criado
USE BazarDB;
GO

--  tabela principal: Produtos
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
--   Nova tabela: Vendas
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
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Vendas') AND name = 'ValorTotal'
)
BEGIN
    ALTER TABLE Vendas
    ADD ValorTotal DECIMAL(10, 2) NOT NULL DEFAULT 0;
 
    PRINT 'Coluna [ValorTotal] adicionada à tabela Vendas.';
END
GO
 IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Vendas') AND name = 'StatusPagamento'
)
BEGIN
    ALTER TABLE Vendas
    ADD StatusPagamento NVARCHAR(50) NOT NULL DEFAULT 'Pago';
 
    PRINT 'Coluna [StatusPagamento] adicionada à tabela Vendas.';
END
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pagamentos' AND xtype='U')
BEGIN
    CREATE TABLE Pagamentos (
        PagamentoID     INT PRIMARY KEY IDENTITY(1,1),
 
        -- A qual venda este pagamento pertence
        VendaID         INT NOT NULL,
 
        -- Quando o dinheiro entrou
        DataPagamento   DATETIME NOT NULL DEFAULT GETDATE(),
         
        -- Quanto foi pago nesta entrada
        ValorPago       DECIMAL(10, 2) NOT NULL,
 
        -- Opcional: forma de pagamento ('Dinheiro', 'PIX', 'Cartão', etc.)
        FormaPagamento  NVARCHAR(50) NULL DEFAULT 'Dinheiro',
 
        -- Observação livre (ex: "Primeira parcela", "Acerto final")
        Observacao      NVARCHAR(255) NULL,
 
        CONSTRAINT FK_Pagamentos_Vendas FOREIGN KEY (VendaID) REFERENCES Vendas(VendaID)
    );
 
    PRINT 'Tabela [Pagamentos] criada com sucesso.';
END
GO
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_SaldoVendas')
    DROP VIEW vw_SaldoVendas;
GO
 
CREATE VIEW vw_SaldoVendas AS
    SELECT
        v.VendaID,
        v.DataVenda,
        v.ValorTotal,
        v.StatusPagamento,
        c.Nome                                          AS NomeCliente,
        ISNULL(SUM(p.ValorPago), 0)                    AS TotalPago,
        v.ValorTotal - ISNULL(SUM(p.ValorPago), 0)    AS ValorRestante
    FROM Vendas v
    LEFT JOIN Clientes c   ON c.ClienteID  = v.ClienteID
    LEFT JOIN Pagamentos p ON p.VendaID    = v.VendaID
    GROUP BY
        v.VendaID, v.DataVenda, v.ValorTotal,
        v.StatusPagamento, c.Nome;
GO
 
PRINT 'View [vw_SaldoVendas] criada com sucesso.';
PRINT '';
PRINT '=== Módulo de Pagamentos Parciais instalado! ===';
GO
USE BazarDB;

-- Actualiza ValorTotal de cada Venda com a soma dos seus itens
UPDATE Vendas
SET ValorTotal = (
    SELECT ISNULL(SUM(iv.Quantidade * iv.PrecoUnitario), 0)
    FROM ItensVenda iv
    WHERE iv.VendaID = Vendas.VendaID
);

-- Confirma o resultado
SELECT VendaID, ValorTotal, StatusPagamento FROM Vendas;
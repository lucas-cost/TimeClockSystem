graph TD
    subgraph " "
        direction LR
        subgraph "UI (WPF)"
            UI[MainWindow.xaml/ViewModel]
        end
        subgraph "Application (Use Cases)"
            APP[Command Handlers]
        end
        subgraph "Core (Business Logic)"
            CORE[Entities & Interfaces]
        end
        subgraph "Infrastructure (External Details)"
            INFRA[Repositories, API Clients, Services]
        end
    end

    subgraph "Execução e Serviços"
        A[Usuário] --> UI
        UI -- Envia Comando --> APP
        APP -- Depende de --> CORE
        INFRA -- Implementa --> CORE
        APP -- Usa (via DI) --> INFRA
        
        subgraph "Serviços de Background"
            BG(SyncWorker)
        end
        
        BG -- Usa (via DI) --> INFRA
    end

    subgraph "Mundo Externo"
        direction TB
        INFRA --> DB[(SQLite DB)]
        INFRA --> API[API Externa]
        INFRA --> HW[Webcam]
        INFRA --> MSG[RabbitMQ]
    end

    style UI fill:#FFF3E6,stroke:#FF9900
    style APP fill:#E6FFE6,stroke:#009900
    style CORE fill:#E6F3FF,stroke:#0066CC
    style INFRA fill:#F2E6FF,stroke:#9900FF
    style BG fill:#FFEBEE,stroke:#D32F2F
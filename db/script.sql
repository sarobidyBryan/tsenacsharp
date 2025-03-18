CREATE TABLE Market (
    id AUTOINCREMENT PRIMARY KEY,
    nom TEXT(50) NOT NULL,
    longueur INTEGER NOT NULL,
    largeur INTEGER NOT NULL,
    x INTEGER NOT NULL,
    y INTEGER NOT NULL
);

CREATE TABLE Histo_Prix(
    id AUTOINCREMENT PRIMARY KEY,
    idMarket INTEGER NOT NULL,
    prix_actuel INTEGER NOT NULL,
    nouveau_prix INTEGER NOT NULL,
    date_changement DATE NOT NULL,
    CONSTRAINT fk_market_Histo_Prix FOREIGN KEY (idMarket) REFERENCES Market(id),
);

CREATE TABLE Box (
    id AUTOINCREMENT PRIMARY KEY,
    reference TEXT(10) NOT NULL,
    longueur INTEGER NOT NULL,
    largeur INTEGER NOT NULL,
    x INTEGER NOT NULL,
    y INTEGER NOT NULL
);

CREATE TABLE Tenant (
    id AUTOINCREMENT PRIMARY KEY,
    nom TEXT(50) NOT NULL
);

CREATE TABLE Location (
    id AUTOINCREMENT PRIMARY KEY,
    idBox INTEGER NOT NULL,
    idTenant INTEGER NOT NULL,
    date_action DATE NOT NULL,
    type_action INTEGER NOT NULL, -- 0->debut 1->fin
    CONSTRAINT fk_box_Location FOREIGN KEY (idBox) REFERENCES Box(id),
    CONSTRAINT fk_tenant_Location FOREIGN KEY (idTenant) REFERENCES Tenant(id)
);

CREATE TABLE Market_Box (
    id AUTOINCREMENT PRIMARY KEY,
    idMarket INTEGER NOT NULL,
    idBox INTEGER NOT NULL,
    CONSTRAINT fk_market_Market_Box FOREIGN KEY (idMarket) REFERENCES Market(id),
    CONSTRAINT fk_box_Market_Box FOREIGN KEY (idBox) REFERENCES Box(id)
);

CREATE TABLE Payement (
    id AUTOINCREMENT PRIMARY KEY,
    idBox INTEGER NOT NULL,
    idTenant INTEGER NOT NULL,
    montant INTEGER NOT NULL,
    mois INTEGER NOT NULL,
    annee INTEGER NOT NULL,
    date_payement DATE NOT NULL,
    CONSTRAINT fk_box_Payement FOREIGN KEY (idBox) REFERENCES Box(id),
    CONSTRAINT fk_tenant_Payement FOREIGN KEY (idTenant) REFERENCES Tenant(id)
);

CREATE TABLE Discount (
    id AUTOINCREMENT PRIMARY KEY,
    mois_debut INTEGER NOT NULL,
    mois_fin INTEGER NOT NULL,
    valeur INTEGER NOT NULL
);

export const SetorAtuacao = {
    Industria: 'Indústria',
    Comercio: 'Comércio',
    Saude: "Saude",
    Tecnologia: "Tecnologia",
    Construcao: "Construcao",
    ServicosGerais: "ServicosGerais",
    Educacao: "Educacao",
    Transporte: "Transporte",
} as const;

export type SetorAtuacao = typeof SetorAtuacao[keyof typeof SetorAtuacao];
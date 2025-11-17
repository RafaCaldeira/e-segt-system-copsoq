import { SetorAtuacao } from "./setor.types";

export interface LoginSuccessResponse {
    message: string;
    token: string;
    userRole:string;
    nomeEmpresa?: string | null;
    empresaId?: number | null;
}

export interface AuthResponse{
    success: boolean;
    token: string | null;
    userRole:string | null;
    nomeEmpresa?: string | null;
    empresaId?: number | null;
    error?: string;
}

export interface RegistroClienteDto{
    email: string;
    senha: string;
    nomeEmpresa: string;
    nomeResponsavel: string;
    setorAtuacao: SetorAtuacao;
    cidade: string;
    cnpj: string;
}
// Corresponde ao modelo C# 'Funcionario.cs'

export interface Funcionario {
  id: number;
  nome: string;
  email: string;
  telefone: string;
  cargo: string;
  setor: string;
  empresaID: number;
  cpf?: string;
  tokenOuId?: string;
}

export interface FuncionarioCreateDto {
  nome: string;
  email: string;
  telefone: string;
  cargo: string;
  setor: string;
  cpf: string;
}
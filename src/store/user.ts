import { defineStore } from 'pinia'

export const useUserStore = defineStore('user', {
  state: () => ({
    empresa: '',
    token: '',
    isAuthenticated: false
  }),
  actions: {
    login(empresa: string, token: string) {
      this.empresa = empresa
      this.token = token
      this.isAuthenticated = true
    },
    logout() {
      this.empresa = ''
      this.token = ''
      this.isAuthenticated = false
    }
  }
})
function loginApp() {
    return {
        credentials: {
            usuario: '',
            password: ''
        },
        loading: false,
        error: '',
        isOffline: false,

        async checkConnection() {
            try {
                const response = await fetch('/api/health', {
                    method: 'GET'
                });
                this.isOffline = !response.ok;
            } catch {
                this.isOffline = true;
            }
        },

        async login() {
            this.loading = true;
            this.error = '';

            try {
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.credentials)
                });

                const data = await response.json();

                if (data.success) {
                    localStorage.setItem('token', data.token);
                    localStorage.setItem('user', JSON.stringify(data.usuario));
                    window.location.href = 'dashboard.html';
                } else {
                    this.error = data.message || 'Credenciales inválidas';
                }
            } catch (err) {
                this.error = 'Error de conexión. Verifica el servidor.';
                console.error('Login error:', err);
            } finally {
                this.loading = false;
            }
        },

        init() {
            // Check if already logged in
            const token = localStorage.getItem('token');
            if (token) {
                window.location.href = 'dashboard.html';
                return;
            }

            this.checkConnection();
            // Check connection every 10 seconds
            setInterval(() => this.checkConnection(), 10000);
        }
    }
}

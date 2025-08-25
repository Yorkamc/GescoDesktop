function dashboardApp() {
    return {
        user: null,
        isOffline: false,
        stats: {
            actividades: 0,
            ventasHoy: 0,
            transacciones: 0
        },
        licenseStatus: {
            isActive: false,
            diasRestantes: 0,
            fechaExpiracion: null
        },
        recentActivity: [],

        async init() {
            // Check authentication
            const token = localStorage.getItem('token');
            const userStr = localStorage.getItem('user');
            
            if (!token || !userStr) {
                window.location.href = 'login.html';
                return;
            }
            
            this.user = JSON.parse(userStr);
            
            // Load initial data
            await this.checkConnection();
            await this.loadStats();
            await this.loadLicenseStatus();
            await this.loadRecentActivity();
            
            // Set up periodic checks
            setInterval(() => this.checkConnection(), 10000);
            setInterval(() => this.loadStats(), 30000);
        },

        async checkConnection() {
            try {
                const response = await fetch('/api/health');
                this.isOffline = !response.ok;
            } catch {
                this.isOffline = true;
            }
        },

        async loadStats() {
            try {
                // Mock data for now - replace with real API calls
                this.stats = {
                    actividades: Math.floor(Math.random() * 10),
                    ventasHoy: Math.floor(Math.random() * 500000),
                    transacciones: Math.floor(Math.random() * 50)
                };
            } catch (err) {
                console.error('Error loading stats:', err);
            }
        },

        async loadLicenseStatus() {
            try {
                const response = await fetch('/api/license/status', {
                    headers: {
                        'Authorization': Bearer 
                    }
                });
                
                if (response.ok) {
                    this.licenseStatus = await response.json();
                }
            } catch (err) {
                console.error('Error loading license status:', err);
            }
        },

        async loadRecentActivity() {
            try {
                // Mock data for now
                this.recentActivity = [
                    {
                        id: 1,
                        description: 'Sesión iniciada correctamente',
                        timestamp: new Date()
                    },
                    {
                        id: 2,
                        description: 'Base de datos local sincronizada',
                        timestamp: new Date(Date.now() - 300000) // 5 minutes ago
                    }
                ];
            } catch (err) {
                console.error('Error loading recent activity:', err);
            }
        },

        async logout() {
            try {
                await fetch('/api/auth/logout', {
                    method: 'POST',
                    headers: {
                        'Authorization': Bearer 
                    }
                });
            } catch (err) {
                console.error('Error during logout:', err);
            } finally {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                window.location.href = 'login.html';
            }
        },

        formatMoney(amount) {
            return new Intl.NumberFormat('es-CR', {
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            }).format(amount || 0);
        },

        formatTime(date) {
            const now = new Date();
            const diff = now - new Date(date);
            const minutes = Math.floor(diff / 60000);
            
            if (minutes < 1) return 'Ahora';
            if (minutes < 60) return ${minutes}m;
            if (minutes < 1440) return ${Math.floor(minutes / 60)}h;
            return ${Math.floor(minutes / 1440)}d;
        }
    }
}

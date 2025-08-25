function activationApp() {
    return {
        activationCode: '',
        organizacionId: '1',
        loading: false,
        error: '',
        success: '',

        formatCode() {
            // Remove non-alphanumeric characters and convert to uppercase
            let code = this.activationCode.replace(/[^A-Z0-9]/g, '').toUpperCase();
            
            // Add dashes every 4 characters
            if (code.length > 0) {
                code = code.match(/.{1,4}/g).join('-');
                if (code.length > 19) { // Max length: XXXX-XXXX-XXXX-XXXX
                    code = code.substr(0, 19);
                }
            }
            
            this.activationCode = code;
        },

        async activate() {
            this.loading = true;
            this.error = '';
            this.success = '';

            try {
                const response = await fetch('/api/license/activate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        codigoActivacion: this.activationCode.replace(/-/g, ''),
                        organizacionId: parseInt(this.organizacionId),
                        dispositivoId: this.getDeviceId(),
                        nombreDispositivo: 'GESCO Desktop'
                    })
                });

                const data = await response.json();

                if (data.success) {
                    this.success = '¡Licencia activada exitosamente! Redirigiendo...';
                    setTimeout(() => {
                        window.location.href = 'login.html';
                    }, 2000);
                } else {
                    this.error = data.message || 'Error al activar la licencia';
                }
            } catch (err) {
                this.error = 'Error de conexión con el servidor';
                console.error('Activation error:', err);
            } finally {
                this.loading = false;
            }
        },

        getDeviceId() {
            let deviceId = localStorage.getItem('deviceId');
            if (!deviceId) {
                deviceId = `desktop-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
                localStorage.setItem('deviceId', deviceId);
            }
            return deviceId;
        },

        init() {
            // Check if already activated and redirect
            this.checkLicenseStatus();
        },

        async checkLicenseStatus() {
            try {
                const response = await fetch('/api/license/status');
                const data = await response.json();
                
                if (data.isActive) {
                    // Already activated, redirect to login
                    window.location.href = 'login.html';
                }
            } catch (err) {
                // Continue with activation if can't check status
                console.log('Could not check license status');
            }
        }
    }
}
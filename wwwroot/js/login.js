document.addEventListener("DOMContentLoaded", function () {
    function switchForm(formId) {
        document.querySelectorAll('.form').forEach(form => {
            form.classList.remove('active');
        });

        document.getElementById(formId).classList.add('active');
    }

    document.querySelectorAll('.switch-form, .forgot-link').forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            switchForm(this.getAttribute('data-form'));
        });
    });

    function mostrarError(error) {
        const estado = error.response?.status;
        const datos = error.response?.data;

        const detalle = datos && typeof datos === 'object'
            ? datos.errorMessage || datos.message
            : null;

        const mensaje = detalle ||
            (error.response
                ? 'El servidor devolvió un error. Código: ' + estado
                : 'No se pudo conectar con el servidor.');

        Swal.fire({
            icon: 'error',
            title: estado === 401 ? 'No se pudo iniciar sesión' : 'Error',
            text: mensaje
        });
    }

    const validate = new JustValidate('#signupForm');

    validate
        .addField('#email', [
            {
                rule: 'required',
                errorMessage: 'El correo es obligatorio'
            },
            {
                rule: 'email',
                errorMessage: 'Ingresa un correo válido'
            }
        ])
        .addField('#password', [
            {
                rule: 'required',
                errorMessage: 'La contraseña es obligatoria'
            }
        ])
        .onSuccess(async event => {
            event.preventDefault();

            const data = {
                Email: document.getElementById("email").value.trim(),
                Password: document.getElementById("password").value
            };

            try {
                const response = await axios.post('/Auth/Login', data);

                if (response.data.success) {
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'success',
                        title: 'Inicio de sesión exitoso',
                        showConfirmButton: false,
                        timer: 1500
                    });

                    setTimeout(() => {
                        window.location.href = '/Home/Index';
                    }, 1500);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'No se pudo iniciar sesión',
                        text: response.data.errorMessage ||
                            'Credenciales inválidas.'
                    });
                }
            } catch (error) {
                mostrarError(error);
            }
        });

    const validateRegisterForm =
        new JustValidate('#registerFormElement');

    validateRegisterForm
        .addField('#UserName', [
            {
                rule: 'required',
                errorMessage: 'El nombre de usuario es obligatorio'
            }
        ])
        .addField('#registerEmail', [
            {
                rule: 'required',
                errorMessage: 'El correo es obligatorio'
            },
            {
                rule: 'email',
                errorMessage: 'Ingresa un correo válido'
            }
        ])
        .addField('#registerPassword', [
            {
                rule: 'required',
                errorMessage: 'La contraseña es obligatoria'
            },
            {
                validator: value =>
                    value.length >= 8 &&
                    value.length <= 128 &&
                    /[a-z]/.test(value) &&
                    /[A-Z]/.test(value) &&
                    /[0-9]/.test(value) &&
                    /[^a-zA-Z0-9\s]/.test(value) &&
                    !/\s/.test(value),

                errorMessage:
                    'Usa entre 8 y 128 caracteres, con mayúscula, ' +
                    'minúscula, número y símbolo, sin espacios'
            }
        ])
        .addField('#confirmPassword', [
            {
                rule: 'required',
                errorMessage: 'Confirma la contraseña'
            },
            {
                validator: (value, fields) =>
                    value === fields['#registerPassword']?.elem?.value,

                errorMessage: 'Las contraseñas no coinciden'
            }
        ])
        .onSuccess(async event => {
            event.preventDefault();

            const data = {
                UserName: document.getElementById("UserName").value.trim(),
                Email: document.getElementById("registerEmail").value.trim(),
                Password: document.getElementById("registerPassword").value
            };

            try {
                const response = await axios.post('/Auth/Register', data);

                if (response.data.success) {
                    await Swal.fire({
                        icon: 'success',
                        title: 'Cuenta creada',
                        text: 'Revisa tu correo para confirmar la cuenta.'
                    });

                    window.location.href = '/Auth/Login';
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'No se pudo registrar',
                        text: response.data.errorMessage ||
                            response.data.message ||
                            'Revisa los datos ingresados.'
                    });
                }
            } catch (error) {
                mostrarError(error);
            }
        });

    const validateForgetForm =
        new JustValidate('#resetPasswordForm');

    validateForgetForm
        .addField('#resetEmail', [
            {
                rule: 'required',
                errorMessage: 'El correo es obligatorio'
            },
            {
                rule: 'email',
                errorMessage: 'Ingresa un correo válido'
            }
        ])
        .onSuccess(async event => {
            event.preventDefault();

            const data = {
                Email: document.getElementById("resetEmail").value.trim()
            };

            try {
                const response = await axios.post(
                    '/Auth/ForgotPassword',
                    data
                );

                if (response.data.success) {
                    await Swal.fire({
                        icon: 'success',
                        title: 'Enlace enviado',
                        text: 'Revisa tu correo para restablecer la contraseña.'
                    });

                    switchForm('signupForm');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'No se pudo enviar el enlace',
                        text: response.data.errorMessage ||
                            response.data.message ||
                            'Intenta nuevamente.'
                    });
                }
            } catch (error) {
                mostrarError(error);
            }
        });
});
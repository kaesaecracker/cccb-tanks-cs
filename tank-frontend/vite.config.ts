import {ConfigEnv, defineConfig} from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig(() => {
    const isContainer = process.env.CONTAINERMODE;
    return {
        plugins: [react()],

        build: {
            outDir: isContainer ? undefined : '../tanks-backend/TanksServer/client',
            emptyOutDir: true
        }
    };
});

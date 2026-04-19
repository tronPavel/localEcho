import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './app/App';
import { QueryProvider } from './app/providers';
import 'leaflet/dist/leaflet.css';
import './app/styles/variables.css';
import './app/styles/animations.css';
import './index.css';
import {BrowserRouter} from "react-router-dom";

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <QueryProvider>
            <BrowserRouter>
                <App />
            </BrowserRouter>
        </QueryProvider>
    </StrictMode>,
);
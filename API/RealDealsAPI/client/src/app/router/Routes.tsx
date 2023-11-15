import { createBrowserRouter, Navigate } from "react-router-dom";
import AboutPage from "../../features/about/AboutPage";
import NotFound from "../errors/NotFound";
import App from "../layout/App";
import MovieCatalog from "../../features/movieCatalog/MovieCatalog";
import ServerError from "../errors/ServerError";
import HomePage from "../../features/home/HomePage";

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
            {path: 'catalog', element: <MovieCatalog />},
            {path: '', element: <HomePage />},
            {path: 'about', element: <AboutPage />},
            {path: 'server-error', element: <ServerError />},
            {path: 'not-found', element: <NotFound />},
            {path: '*', element: <Navigate replace to='/not-found' />}
        ]
    }
])
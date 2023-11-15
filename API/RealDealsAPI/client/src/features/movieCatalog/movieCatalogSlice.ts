import { BestDeal } from './../../app/models/bestdeal';
import { router } from './../../app/router/Routes';
import { createAsyncThunk, createEntityAdapter, createSlice } from "@reduxjs/toolkit";
import { Movie } from "../../app/models/movie";
import { RootState } from "../../app/store/configureStore";
import agent from "../../app/api/agent";

interface MovieCatalogState {
    moviesLoaded: boolean;
    status: string;
    // bestDealDetailsLoaded: boolean;
    bestDealLoadStatus: string;
    bestDealDetails : BestDeal | null;
}

const moviesAdapter = createEntityAdapter<Movie>(
    {
        selectId: (movie) => movie.title,
        sortComparer: (a, b) => a.title.localeCompare(b.title),
    }
);

export const fetchMoviesAsync = createAsyncThunk<Movie[], void, { state: RootState }>(
    'moviecatalog/fetchMoviesAsync',
    async (_, thunkAPI) => {
        try {
            const response = await agent.MovieCatalog.list();
            return response;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } catch (error: any) {
            return thunkAPI.rejectWithValue({ error: error.data })
        }
    }
)

export const fetchBestDealAsync = createAsyncThunk<BestDeal, string>(
    'catalog/fetchBestDealAsync',
    async (relatedIds, thunkAPI) => {
        try {
            const bestDeal = await agent.MovieCatalog.GetBestDeal(relatedIds);
            return bestDeal;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } catch (error: any) {
            return thunkAPI.rejectWithValue({ error: error.data })
        }
    }
)

export const movieCatalogSlice = createSlice({
    name: 'movieCatalog',
    initialState: moviesAdapter.getInitialState<MovieCatalogState>({
        moviesLoaded: false,
        status: 'idle',
        // bestDealDetailsLoaded: false,
        bestDealLoadStatus: 'idle',
        bestDealDetails : null,
    }),
    reducers: {
        clearBestDealDetails: (state) => {
            state.bestDealDetails = null
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchMoviesAsync.pending, (state) => {
            state.status = 'pendingFetchMovies'
        });
        builder.addCase(fetchMoviesAsync.fulfilled, (state, action) => {
            moviesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.moviesLoaded = true;
        });
        builder.addCase(fetchMoviesAsync.rejected, (state) => {
            router.navigate('/server-error');
            state.status = 'idle';
        });
        builder.addCase(fetchBestDealAsync.pending, (state) => {
            state.bestDealLoadStatus = 'pendingFetchBestDeal';
        });
        builder.addCase(fetchBestDealAsync.fulfilled, (state, action) => {
            state.bestDealDetails = action.payload;
            state.bestDealLoadStatus = 'idle';
        });
        builder.addCase(fetchBestDealAsync.rejected, (state) => {
            router.navigate('/not-found');
            state.status = 'idle';
        });
    })
})


export const movieSelectors = moviesAdapter.getSelectors((state: RootState) => state.movieCatalog);

export const {clearBestDealDetails} = movieCatalogSlice.actions;
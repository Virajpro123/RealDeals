import { Backdrop, Box, CircularProgress, Typography } from "@mui/material";
import * as React from 'react';

interface Props {
    message?: string;
    isBackDropInvisible: boolean;
}

export default function LoadingComponent({ message = 'Loading...', isBackDropInvisible }: Props) {
    return (
        <Backdrop open={true} style={{backgroundColor:'secondary'}} invisible={isBackDropInvisible} data-testid="backdrop">
            <Box  bgcolor="white" alignItems='center' display='flex' justifyContent='center' width= '50vh' height='50vh'>
                <CircularProgress size={100} color='secondary' />
                <Typography variant='h4' sx={{ justifyContent: "center", position: "fixed", top: "60%" }}>{message}</Typography>
            </Box>
        </Backdrop>
    )
}


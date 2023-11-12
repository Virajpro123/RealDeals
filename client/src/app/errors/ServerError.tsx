import { Container, Divider, Paper, Typography } from "@mui/material";

export default function ServerError() {

    return (
        <Container component={Paper}>
            <>
                <Typography gutterBottom variant="h3" color='secondary'>
                    Internal server error
                </Typography>
                <Divider />
            </>
            <Typography gutterBottom variant='h5'>Please Try Again Later..</Typography>
        </Container>
    )
}
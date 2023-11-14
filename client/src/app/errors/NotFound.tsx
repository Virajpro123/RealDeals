import { Container, Paper, Typography, Divider, Button } from "@mui/material";
import { Link } from "react-router-dom";

export default function NotFound() {
    return (
        <Container component={Paper} style={{height: 400}}>
            <Typography gutterBottom variant={'h3'}>Oops - we could not find what you are looking for!!</Typography>
            <Divider />
            <Button style={{color: '#FFFFFF', background : '#1A2C50'} } component={Link} variant="contained" to='/' fullWidth>Go back to the Home</Button>
        </Container>
    )
}
import axios from "axios";

axios.defaults.baseURL = 'http://localhost:5000/api/';
// axios.defaults.withCredentials = true;

const responseBody = (response: AxiosResponse) => response.data;

const requests = {
    get: (url: string, params?: URLSearchParams) => axios.get(url, {params}).then(responseBody),
    post: (url: string, body: {}) => axios.post(url, body).then(responseBody),
    put: (url: string, body: {}) => axios.put(url, body).then(responseBody),
    delete: (url: string) => axios.delete(url).then(responseBody),
    postForm: (url: string, data: FormData) => axios.post(url, data, {
        headers: {'Content-type': 'multipart/form-data'}
    }).then(responseBody),
    putForm: (url: string, data: FormData) => axios.put(url, data, {
        headers: {'Content-type': 'multipart/form-data'}
    }).then(responseBody)
}

const MovieCatalog = {
    list: () => requests.get('movies'),
    GetBestDeal: (relatedIds: string) => requests.get(`movies/getbestdeal?relatedIds=${relatedIds}`)
}

const agent = {
    MovieCatalog
}

export default agent;



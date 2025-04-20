# ModularMonolith.NET
A simple modular monolith implementation for ASP.NET

# Labels

A Record Label is a company that signs Artists and helps them produce, distribute, and promote their music.

## Model 
Here's a simplified model for a Label:

```json
{
  "uuid": "<uuidv4>",
  "name": "Warp Records",
  "status": "Active",
  "artists":[
    {
      "uuid": "<uuidv4>",
      "name": "Aphex Twin"
    }
  ],
  "created_at": "1/1/1970",
  "changed_at": "1/1/1970"
}
```

Artist names are denormalized for performance reasons.  We'll keep them in sync using events.

The list of Artists for the Label is considered the 'source of truth' regarding which Artists are currently signed to the Label.

## Operations

* We can return a list of Labels and their signed Artists
* We can create a new Label
* We can change the name of a Label
* We can sign an Artist
* We can drop an Artist

## Events

### Raised

* `LABEL_CREATED`: Raised when a new Label is created
* `LABEL_CHANGED`: Raised an existing Label record is changed in any way
* `LABEL_ARTISTS_CHANGED`: Raised when the collection of Label Artists is modified

Note that there's no `LABEL_ARTIST_SIGNED` or `LABEL_ARTIST_DROPPED`; those events are considered "stateful", and because our message bus doesn't guarantee the ordering of events, we can't use stateful events (or stateful information in event bodies) without risking desynchronized data.

### Subscribed

* `ARTIST_CHANGED`: Update the associated Artist's name

# Artists

An Artist is a musician or musical group that creates and performs music, often releasing tracks and albums under a record label.

## Model

```json
{
  "uuid": "<uuidv4>",
  "name": "Aphex Twin",
  "label": {
    "uuid": "<uuidv4>",
    "name": "Warp Records"
  },
  "albums": [
    {
      "uuid": "<uuidv4>",
      "name": "Selected Ambient Works 85-92",
      "year": 1992
    }
  ],
  "created_at": "1/1/1970",
  "changed_at": "1/1/1970"
}
```

Label name and Album names and years are denormalized for performance reasons.  We'll keep them in sync using events.

The list of Albums for the Artist is considered the 'source of truth' regarding which Albums were released by the Artist.

## Operations

* We can create a new Artist
  * Initially not signed to a Label
* We can change the name of an Artist
* We can add a new Album to the Artist

## Events

* `ARTIST_CREATED`: Raised when a new Artist is created
* `ARTIST_CHANGED`: Raised when an existing Artist record is changed in any way
* `ARTIST_ALBUMS_CHANGED`: Raised when the collection of Artist Albums is modified

### Raised

### Subscribed

* `LABEL_UPDATED`: Update the Label's name on any signed Artists
* `LABEL_ARTISTS_CHANGED`: Synchronize the Label's list of signed Artists, ensuring that all signed Artists have the correct `label` information, and that no unsigned Artists are associated with the Label

# Album

An Album is a collection of audio recordings released together as a unified body of work by an Artist.

## Model

```json
{
  "uuid": "<uuidv4>",
  "name": "Selected Ambient Works 85-92",
  "artist": {
    "uuid": "uuidv4",
    "name": "Aphex Twin"
  },
  "year": 1992,
  "tracks": [
    {
      "number": 1,
      "name": "Xtal",
      "length": 291
    },
    {
      ...
    }
  ],
  "created_at": "1/1/1970",
  "updated_at": "1/1/1970"
}
```

Artist name is denormalized for performance reasons.

## Operations

* We can create a new Album
  * Initially not associated with an Artist
* We can change the name of an Album
* We can change the year of the Album
* We can update the track listing, order, names, and track lengths

## Events

### Raised

* `ALBUM_CREATED`: Raised when a new Album is created
* `ALBUM_UPDATED`: Raised when an existing Album record is updated in any way

### Subscribed

* `ARTIST_UPDATED`: Update the Artist's name on any associated Albums
* `ARTIST_ALBUMS_CHANGED`: Synchronize the Artist's list of Albums, ensuring that all Albums that appear in the Artist's list of albums are associated with that Artist, and that no albums that are excluded from the list are